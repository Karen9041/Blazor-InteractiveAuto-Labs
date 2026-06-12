// 模擬點擊隱藏的 InputFile 元件
export function clickElement(elementId) {
    const el = document.getElementById(elementId);
    if (el) {
        el.click();
    }
}