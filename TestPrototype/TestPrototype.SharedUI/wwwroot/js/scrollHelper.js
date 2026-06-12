export function enableHorizontalScroll(el) {
    if (el) {
        console.log("成功綁定橫向捲動元素:"); // 除錯用

        el.addEventListener('wheel', (evt) => {
            // 只要不是完全的橫向滾動，我們就攔截它
            if (evt.deltaY !== 0) {
                evt.preventDefault(); // 阻止原本的上下滾動
                el.scrollLeft += (evt.deltaY*0.2); // 轉換成左右滾動
            }
        }, { passive: false });
    } else {
        console.warn("找不到元素 ID:", el);
    }
}