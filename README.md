# Blazor InteractiveAuto Labs 

本儲存庫為 **Blazor InteractiveAuto (SSR + WASM 混合模式)** 的實驗與概念驗證（PoC）專案，專注於研究混合渲染模式下的水合現象（Hydration）、狀態同步、生命週期控管以及極致的使用者體驗優化。

## 🧪 已實現之技術實驗清單

下方列出專案中針對各種技術痛點所進行的實驗與實作場景：

| 技術主題 / 機制 | 實驗目的與解決痛點 | 核心實作位置 |
| :--- | :--- | :--- |
| **InteractiveAuto 生命週期** | 驗證兩端生命週期重複執行機制，進行正確的資料抓取控管。 | `App.razor` / 各頁面組件 |
| **Hydration 抗閃爍動畫** | 透過環境感知，在 WASM 水合接管前鎖定樣式，避免 UI 閃爍。 | `MainLayout.razor` (`isHydrating`) |
| **網址 Ticket 靜默登入** | 訪客進入時，由瀏覽器背景靜默換取 Cookie 身分，並自動清理網址。 | `AuthGateKeeper.razor` |
| **多帳號身分衝突攔截** | 當瀏覽器已有人登入卻又帶入新 Ticket 時，攔截並顯示衝突解決彈窗。 | `AuthService.cs` / `LoginPrompt` |
| **BFF 雙端偏好同步** | 結合 GDPR 同意機制，將雲端 Theme/i18n 偏好於 SSR/WASM 雙端無縫寫入。 | `PreferenceService` 實作 |
| **全域 UI 狀態機包裹器** | 打造 `<StateBoundary>`，統一處理 Loading、Success、Empty、Error 四大狀態。 | `StateBoundary.razor` |
| **骨架屏與 CLS 防治** | 實作 Skeleton 佔位，抽離 JS 依賴至 Service，消滅累積版面配置轉移 (CLS)。 | `PostCardSkeleton.razor` |
| **SSR 水合狀態無縫繼承** | 透過 `PersistentComponentState` 將 SSR 首屏資料轉交 WASM，達成零閃爍接管。 | `CustomAuthStateProvider.cs` |

## 🌟 核心架構亮點 (Core Architecture Highlights)

### 1. 零信任身分驗證與狀態管理架構 (Zero-Trust Auth & Security)

本專案建立了一套高內聚的身分安全體系，確保使用者身分在 SSR 與 WASM 切換間始終保持一致且不受劫持。

#### 身分權威機制 (Identity Authority)

- **CustomAuthStateProvider:** 作為全域身分保險箱，嚴格控管 AuthenticationState。透過與後端 API 緊密綁定，確保身分驗證結果在 InteractiveAuto 雙端渲染時的絕對一致性，徹底解決了 SSR 與 WASM 因不同步導致的身分閃爍問題。

- **AuthGateKeeper (靜默登入海關):** 針對訪客進入網頁的場景，實作了背景靜默換票機制。系統能自動辨識網址參數中的臨時 Ticket，在不打擾使用者的情況下完成身分校驗，並自動清理網址列，維持乾淨的 URI。

#### 異常防護與衝突攔截

- **多帳號衝突偵測:** 當偵測到瀏覽器已存在舊的登入憑證，卻又接收到新的身分請求時，系統會立即觸發「身分衝突保護機制」，攔截並強制要求使用者決策（保留舊身分或切換為新身分），防範會話劫持風險。

- **嚴謹的 AuthGuard:** 封裝了 AuthService 作為權限守門員，任何涉及敏感操作（如發文、點讚）的元件，必須透過 RequireLoginAsync 的強檢查，確保未登入使用者無法非法觸發後端 API。

#### 架構依賴關係圖

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
```

### 2. 智慧型雙端偏好同步與隱私防護 (BFF & GDPR Compliant Preference Sync)

本專案針對 Blazor InteractiveAuto 模式，打造了極度嚴密的使用者偏好（Theme、i18n）同步架構，確保跨裝置體驗無縫接軌且符合現代隱私法規。

#### 隱私法規守門員 (Cookie Consent Gatekeeper)

所有的 UX Cookie 寫入動作均由底層的 `IPreferenceService` 進行攔截。
- **Non-Essential Cookies (Theme, Culture)：** 必須在使用者點擊「同意」橫幅（寫入 `cookie_consent=true`）後，系統才允許將偏好寫入瀏覽器。若未同意，系統依然正常運作，但退回系統預設且不留痕跡。
- **Strictly Necessary Cookies (AccessToken)：** 由後端 API 透過 `HttpOnly` Response Header 直接寫入，免疫 XSS 攻擊且不被同意橫幅阻擋，確保核心驗證邏輯暢通。

#### Two-Step Fetch 登入同步機制

為了確保乾淨的 API 職責分離：
1. **驗證階段：** 前端呼叫 `/api/mock/login` 僅換取乾淨的 `Token`。
2. **偏好拉取：** 攔截器立刻攜帶 Token 呼叫 `/api/mock/me` 獲取完整 `UserDto`。
3. **智慧重整 (Smart Reload)：** 系統比對雲端偏好與本地 Cookie 差異，僅在「語系 (Culture)」真正發生改變時，才觸發 `forceLoad: true`，大幅減少不必要的畫面閃爍。

#### 雙端寫入策略 (InteractiveAuto Compatibility)

- **SSR 階段登入：** 透過 `ServerPreferenceService` 注入 `IHttpContextAccessor`，將設定附加於 HTTP Response Header 送回客戶端，避免 Hydration 閃爍。
- **WASM 階段登入：** 透過 `ClientPreferenceService` 呼叫原生 JavaScript (`cookieHelper` / `themeHelper`)，達成瞬間的主題熱切換。

#### 架構依賴關係圖

```mermaid
flowchart LR
    %% 定義樣式
    classDef trigger fill:#e3f2fd,stroke:#2196f3,stroke-width:2px,color:#000000;
    classDef guard fill:#fff3e0,stroke:#ff9800,stroke-width:2px,color:#000000;
    classDef execution fill:#e8f5e9,stroke:#4caf50,stroke-width:2px,color:#000000;
    classDef action fill:#f3e5f5,stroke:#9c27b0,stroke-width:2px,color:#000000;

    %% 觸發點
    subgraph Triggers [觸發來源]
        UI[使用者點擊 Theme/Lang 選單]:::trigger
        Auth[AuthService 登入 Two-Step Fetch]:::trigger
    end

    %% 守門員機制
    subgraph Guard [IPreferenceService 隱私守門員]
        CallSet[呼叫 SetValueAsync]
        CheckConsent{檢查 cookie_consent?}:::guard
        Abort((中斷寫入<br/>捨棄偏好))
    end

    %% 雙端寫入執行期
    subgraph Runtime ["雙端寫入策略 (InteractiveAuto)"]
        SSR[ServerPreferenceService<br/>透過 HttpResponse.Cookies.Append]:::execution
        WASM[ClientPreferenceService<br/>透過 JSInterop cookieHelper.set]:::execution
    end

    %% 畫面反應
    subgraph UIRender [畫面更新機制]
        Theme["主題: 呼叫 themeHelper.setTheme<br/>瞬間變色 (無閃爍)"]:::action
        Culture["語系: 標記 requiresReload<br/>強制重整 (forceLoad: true)"]:::action
    end

    %% 流程連線
    UI --> CallSet
    Auth -- "解析 UserDto" --> CallSet
    CallSet --> CheckConsent

    CheckConsent -- "未同意 / 拒絕" --> Abort
    CheckConsent -- "值為 true" --> Route{判斷當前渲染模式}

    Route -- "SSR 階段" --> SSR
    Route -- "WASM 階段" --> WASM

    SSR --> SyncCheck{判斷設定類型}
    WASM --> SyncCheck

    SyncCheck -- "如果是 Theme" --> Theme
    SyncCheck -- "如果是 Culture" --> Culture
```
### 3. 全站 UI 狀態機與無縫水合 (Global UI State Machine & Seamless Hydration)
為解決 InteractiveAuto 模式下最棘手的「水合閃爍 (Hydration Flicker)」與「版面吞噬 (CLS)」，本專案導入了標準化的狀態服務與骨架屏機制。

#### 泛型狀態包裹器 (StateBoundary)
設計 `<StateBoundary>` 元件，統一管理非同步載入的四大生命週期：
- **Loading:** 顯示具備脈動動畫的骨架屏 (Skeleton)，安撫等待焦慮並佔位防止 CLS。
- **Success:** 渲染真實資料。
- **Empty / Error:** 處理空狀態引導與例外防呆重試機制。

#### SSR 資料遺產繼承 (PersistentComponentState)
完美解決 SEO、UX 與 API 請求浪費的三角難題。系統會根據使用者的「進入路徑」智能決定渲染策略。

#### 水合無縫接軌流程圖 (Hydration Sequence)

```mermaid
sequenceDiagram
    participant C as Client (Browser)
    participant S as Server (SSR)
    participant WASM as WebAssembly

    Note over C, S: 情境 A：首次載入或重新整理 (首屏 SEO)
    C->>S: 1. 請求網頁 (e.g., 首頁)
    S->>S: 2. 執行 OnInitializedAsync() 抓取真實資料
    S->>S: 3. 觸發 PersistData 打包資料為 JSON
    S-->>C: 4. 回傳包含真實 DOM 與隱藏 JSON 的 HTML
    Note over C: 使用者瞬間看到完整內容<br/>(SEO 滿分，無骨架屏閃爍)
    
    C->>WASM: 5. 背景下載 .NET 執行環境
    WASM->>WASM: 6. 啟動並二次執行 OnInitializedAsync()
    WASM->>WASM: 7. TryTakeFromJson 發現 SSR 遺產
    Note over WASM: 沿用資料，跳過 API 請求<br/>實現 0 閃爍的 SPA 接管
    
    Note over C, WASM: 情境 B：WASM 接管後的站內路由跳轉
    C->>WASM: 8. 點擊導覽列前往其他頁面
    WASM->>WASM: 9. 無 SSR 遺產，觸發 UIState.Loading
    Note over WASM: 顯示 Skeleton 骨架屏<br/>純前端發起 API 請求
```