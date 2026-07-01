export async function shareNative(title, text, url) {
    const shareData = { title, text, url };

    // 檢查是否支援 Web Share API
    if (navigator.share && navigator.canShare && navigator.canShare(shareData)) {
        try {
            await navigator.share(shareData);
            return true; // 分享成功
        } catch (error) {
            // 不管是使用者取消 (AbortError) 還是其他錯誤，一律回傳 false
            console.warn('原生分享中斷或失敗:', error);
            return false;
        }
    }

    // 瀏覽器不支援
    return false;
}

export async function copyToClipboard(text) {
    try {
        if (navigator.clipboard) {
            await navigator.clipboard.writeText(text);
            return true;
        }
        return false;
    } catch (error) {
        console.error('剪貼簿 API 失敗:', error);
        return false;
    }
}
