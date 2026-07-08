using System.Net.Http.Json;
using System.Text.RegularExpressions;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public class PostUploadService
    {
        private readonly HttpClient _httpClient;

        public PostUploadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 核心複合發文方法：自動處理圖片直傳 GCS ➔ 封裝貼文資料 ➔ 後端儲存
        /// </summary>
        public async Task<PostResponse?> PublishCompletePostAsync(PostDto newPost)
        {
            string? tempObjectName = null;

            // 1. 檢查是否有前端產出的 Base64 圖片資料需要上傳
            if (!string.IsNullOrEmpty(newPost.Base64Image))
            {
                // 解析出純 Base64 字串與 Content-Type
                var match = Regex.Match(newPost.Base64Image, @"^data:(?<type>image\/\w+);base64,(?<data>.+)");
                if (match.Success)
                {
                    string contentType = match.Groups["type"].Value;
                    string base64Data = match.Groups["data"].Value;
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    // 2. 向後端索取 GCS Signed URL
                    var urlRequest = new { ContentType = contentType };
                    var urlResponse = await _httpClient.PostAsJsonAsync("api/Post/generate-upload-url", urlRequest);

                    if (urlResponse.IsSuccessStatusCode)
                    {
                        var signResult = await urlResponse.Content.ReadFromJsonAsync<UploadURLResponse>();
                        if (signResult != null && !string.IsNullOrEmpty(signResult.UploadUrl))
                        {
                            // 3. 將圖片以 Stream 方式直接 PUT 到 GCS 暫存區
                            using var ms = new MemoryStream(imageBytes);
                            using var content = new StreamContent(ms);
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                            var uploadToGcsResult = await _httpClient.PutAsync(signResult.UploadUrl, content);
                            if (uploadToGcsResult.IsSuccessStatusCode)
                            {
                                // 成功取得在 GCS 上的暫存位置
                                tempObjectName = signResult.ObjectName;
                            }
                        }
                    }
                }
            }

            // 4. 封裝成最終的 PostRequest，傳送給後端 add 路由
            var finalRequest = new PostRequest
            {
                UserId = newPost.AuthorId,
                Content = newPost.Content,
                TempObjectName = tempObjectName, // 帶入剛才上傳完成的暫存路徑
                Tags = newPost.Tags,
                ExtraValues = null
            };

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7122/api/Post/add", finalRequest);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PostResponse>();
            }

            return new PostResponse { IsSuccess = false };
        }

        private class UploadURLResponse
        {
            public string UploadUrl { get; set; } = "";
            public string ObjectName { get; set; } = "";
        }
    }
}
