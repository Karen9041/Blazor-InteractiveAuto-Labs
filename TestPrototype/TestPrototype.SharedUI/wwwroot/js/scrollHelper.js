export function enableHorizontalScroll(el) {
    if (el) {
        el.addEventListener('wheel', (evt) => {
            // 只要不是完全的橫向滾動，我們就攔截它
            if (evt.deltaY !== 0) {
                evt.preventDefault(); // 阻止原本的上下滾動
                el.scrollLeft += (evt.deltaY*0.2); // 轉換成左右滾動
            }
        }, { passive: false });
    } else {
        console.warn("element not found:", el);
    }
}
