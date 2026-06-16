# Blazor InteractiveAuto Labs 

本儲存庫為 **Blazor InteractiveAuto (SSR + WASM 混合模式)** 的實驗與概念驗證（PoC）專案，專注於研究混合渲染模式下的水合現象（Hydration）、狀態同步、生命週期控管以及極致的使用者體驗優化。

## 🧪 已實現之技術實驗清單

下方列出專案中針對各種技術痛點所進行的實驗與實作場景：

| 技術主題 / 機制 | 實驗目的與解決痛點 | 核心實作位置 |
| :--- | :--- | :--- |
| **InteractiveAuto 生命週期** | 驗證兩端生命週期重複執行機制，進行正確的資料抓取控管。 | `App.razor` / 各頁面組件 |
| **Hydration 抗閃爍動畫** | 透過環境感知（IsBrowser），在 WASM 水合接管前鎖定樣式，避免 UI 閃爍。 | `MainLayout.razor` (`isHydrating`) |
| **Persistent Component State** | 將 SSR 階段抓取好的資料封裝傳遞至 WASM，防治 Hydration 資料二次載入閃爍。 | `CustomAuthStateProvider.cs` |
| **網址 Ticket 背景靜默登入** | 訪客進入時，由瀏覽器背景靜默換取 Cookie 身分，並自動清理網址列參數。 | `AuthGateKeeper.razor` |
| **多帳號身分衝突攔截** | 當瀏覽器已有人登入卻又帶入新 Ticket 時，攔截流程並顯示衝突解決彈窗。 | `AuthGateKeeper.razor` / `AccountConflictModal` |

## 🔐 身分驗證與狀態管理架構 (BFF Auth Architecture)

本專案採用 Blazor Auto Render Mode，並實作了基於 BFF (Backend-for-Frontend) 模式的嚴謹身分驗證機制。透過 `PersistentComponentState` 解決了 SSR 到 WASM 的 Hydration 狀態閃爍問題，並實現了極高內聚的權限攔截系統。

### 架構依賴關係圖

```mermaid
graph TD
    %% 定義樣式
    classDef ui fill:#e3f2fd,stroke:#1565c0,stroke-width:2px,color:#000000;
    classDef service fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px,color:#000000;
    classDef core fill:#fff3e0,stroke:#e65100,stroke-width:2px,color:#000000;
    classDef api fill:#fce4ec,stroke:#c2185b,stroke-width:2px,color:#000000;

    %% 核心服務
    CustomAuthStateProvider["CustomAuthStateProvider<br/>(狀態保險箱 / 身分權威)"]:::core
    AuthService["AuthService : IAuthService<br/>(驗證動作中樞 / AuthGuard)"]:::service
    LoginModalService["LoginModalService<br/>(全域彈窗廣播)"]:::service

    %% UI 元件
    PublishWidget["PublishWidget, Notifications<br/>(高內聚特性元件)"]:::ui
    LoginPrompt["LoginPrompt.razor<br/>(全域遮罩彈窗)"]:::ui
    AuthGateKeeper["AuthGateKeeper.razor<br/>(靜默登入海關)"]:::ui

    %% 外部系統
    BackendAPI["Backend API<br/>(Minimal API)"]:::api
    PersistState[("PersistentComponentState<br/>(SSR 狀態流)")]:::core

    %% --- 依賴與呼叫關係 ---
    PublishWidget -- "1. 動作前呼叫 RequireLoginAsync()" --> AuthService
    AuthGateKeeper -- "呼叫 SilentLogin()" --> AuthService

    AuthService -- "2. 詢問狀態" --> CustomAuthStateProvider
    AuthService -- "3. 未登入攔截：呼叫 Show()" --> LoginModalService
    AuthService -- "驗證動作 (Login/Logout)" --> BackendAPI

    CustomAuthStateProvider -- "SSR 寫入 / WASM 讀取" --> PersistState
    CustomAuthStateProvider -- "無快取時：驗證 Cookie" --> BackendAPI

    LoginModalService -. "4. 觸發 OnChange 事件" .-> LoginPrompt