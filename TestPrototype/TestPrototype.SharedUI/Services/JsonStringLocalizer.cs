using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace TestPrototype.SharedUI.Services
{
    /*負責在元件需要翻譯時，瞬間從 dll 中把 JSON 抽出來並解析*/
    public class JsonStringLocalizer : IStringLocalizer
    {
        // 加入 static 快取，避免重複讀取 DLL 浪費效能
        private static readonly Dictionary<string, Dictionary<string, string>> _cache = new();
        private readonly Dictionary<string, string> _localization;

        public JsonStringLocalizer()
        {
            // 動態取得當下的語言 (SSR 是來自 Request，WASM 是來自 Program.cs 的設定)
            var cultureName = CultureInfo.CurrentUICulture.Name;

            if (!_cache.ContainsKey(cultureName))
            {
                _cache[cultureName] = LoadJson(cultureName);
            }

            _localization = _cache[cultureName];
        }

        private Dictionary<string, string> LoadJson(string cultureName)
        {
            // 組合資源路徑 (Namespace.Folder.Filename)
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"TestPrototype.SharedUI.i18n.{cultureName}.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);

            // 如果找不到該語系的檔案 (例如有人切換到日文但我們沒提供)，退回預設中文
            if (stream == null)
            {
                var defaultStream = assembly.GetManifestResourceStream("TestPrototype.SharedUI.i18n.zh-TW.json");
                if (defaultStream == null) return new Dictionary<string, string>();
                return JsonSerializer.Deserialize<Dictionary<string, string>>(defaultStream) ?? new();
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? new();
        }

        // IStringLocalizer 的核心：透過 Key 拿 Value
        public LocalizedString this[string name]
        {
            get
            {
                var value = _localization.TryGetValue(name, out var val) ? val : name;
                return new LocalizedString(name, value, value != name);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = this[name].Value;
                var value = string.Format(format, arguments);
                return new LocalizedString(name, value, this[name].ResourceNotFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _localization.Select(l => new LocalizedString(l.Key, l.Value, true));
        }
    }
}