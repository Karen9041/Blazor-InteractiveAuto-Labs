window.themeHelper = {
    // 取得系統目前的偏好 (深色回傳 'dark'，否則 'light')
    getSystemTheme: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    },

    // 套用主題到 HTML 標籤上
    applyTheme: function (themePreference) {
        let finalTheme = themePreference;

        // 如果使用者選擇「跟隨系統」或根本沒設定
        if (!themePreference || themePreference === 'system') {
            finalTheme = this.getSystemTheme();
        }

        document.documentElement.setAttribute('data-theme', finalTheme);
    },

    // 啟動監聽器：當作業系統設定改變時，即時更新網頁
    watchSystemTheme: function () {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            // 先檢查使用者的 Cookie，如果是強制指定亮暗，就不理會系統變更
            let currentPref = window.cookieHelper.get('theme');
            if (!currentPref || currentPref === 'system') {
                const newTheme = e.matches ? 'dark' : 'light';
                document.documentElement.setAttribute('data-theme', newTheme);
            }
        });
    }
};

// 頁面載入時自動啟動監聽
window.themeHelper.watchSystemTheme();