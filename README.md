# Blazor InteractiveAuto Labs 

本儲存庫為 **Blazor InteractiveAuto (SSR + WASM 混合模式)** 的實驗與概念驗證（PoC）專案，專注於研究混合渲染模式下的水合現象（Hydration）、狀態同步、生命週期控管以及極致的使用者體驗優化。

## 🧪 已實現之技術實驗清單

下方列出專案中針對各種技術痛點所進行的實驗與實作場景：

| 技術主題 / 機制 | 實驗目的與解決痛點 | 核心實作位置 |
| :--- | :--- | :--- |
| **InteractiveAuto 生命週期** | 驗證兩端生命週期重複執行機制，進行正確的資料抓取控管。 | `App.razor` / 各頁面組件 |
| **Hydration 抗閃爍動畫** | 透過環境感知（IsBrowser），在 WASM 水合接管前鎖定樣式，避免 UI 閃爍。 | `MainLayout.razor` (`isHydrating`) |
| **Persistent Component State** | 將 SSR 階段抓取好的資料封裝傳遞至 WASM，防治 Hydration 資料二次載入閃爍。 | `[待確定] ApplicationStateRepository` |
| **網址 Ticket 背景靜默登入** | 訪客進入時，由瀏覽器背景靜默換取 Cookie 身分，並自動清理網址列參數。 | `SilentLoginDemo.razor` (`OnAfterRenderAsync`) |
| **多帳號身分衝突攔截** | 當瀏覽器已有人登入卻又帶入新 Ticket 時，攔截流程並跳出 Modal 供使用者抉擇。 | `MainLayout.razor` (`ConflictService`) |
| **自訂 AuthStateProvider 廣播** | 靜默登入成功後，手動觸發安全性狀態變更廣播，即時重新渲染受保護的 UI。 | `CustomAuthStateProvider.cs` |
| **全域事件訂閱與記憶體控管** | 實作跨組件事件監聽（如網址改變自動關表單），並透過 `IDisposable` 防治記憶體洩漏。 | `MainLayout.razor` (`Dispose()`) |
| **Cookie 使用者偏好持久化** | 實驗利用 Cookie 儲存 Dark Mode 或語系，使 SSR 與 WASM 兩端皆能無縫讀寫。 | `[待確定] PreferenceService` |

## 🛠️ 開發環境與架構
* **Framework:** .NET 8.0 / .NET 9.0 (Blazor Web App)
* **Render Mode:** InteractiveAuto (Prerendering Enabled)
