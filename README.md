# 點餐系統 Ordering System

> A modern online ordering system built with ASP.NET Core MVC, designed for efficient menu management, flexible ordering, and administrative control.

---

## 📌 專案簡介

本系統為一套具備「前台點餐」與「後台管理」功能的線上點餐平台，支援餐點分類、加價選項、多選與單選設定，並結合訂單追蹤與管理功能，適合用於小型餐廳、飲料店、自助餐點等場景。


## 💡 系統功能

### 🔸 前台（User Area）

    * 🔎 餐點瀏覽與分類篩選
    * ➕ 加入購物車與客製化選項
    * 🛒 購物車管理與即時價格計算
    * 📦 結帳流程（含未登入自動提醒）
    * 📜 訂單查詢與狀態追蹤
    * 📑 收據預覽與列印
    * 👤 會員註冊、登入、密碼修改、個人資料管理

### 🔹 後台（Admin Area）

    * 📊 儀表板統計（每日／週／月／熱門餐點）
    * 📂 分類管理（排序、拖曳調整）
    * 🍱 餐點管理（CRUD、圖片上傳、複製、啟用停用）
    * 🧩 選項與加價項目管理
    * 📑 訂單後台（狀態變更、過濾、明細檢視）
    * 📈 銷售統計 API

---

🛠️ 技術棧
後端技術

    Framework: ASP.NET Core 8.0 MVC
    
    ORM: Entity Framework Core
    
    Database: SQL Server / MySQL / PostgreSQL
    
    Authentication: ASP.NET Core Identity
    
    Dependency Injection: Built-in DI Container

前端技術

    UI Framework: Bootstrap 5.3
    
    JavaScript: Vanilla JS + jQuery
    
    Icons: Font Awesome
    
    Charts: Chart.js (統計圖表)

開發工具

    IDE: Visual Studio 2022 / VS Code
    
    Version Control: Git
    
    Package Manager: NuGet
    
    Database Tools: SQL Server Management Studio

🚀 快速開始
環境要求

    .NET 8.0 SDK
    SQL Server 2019+ / MySQL 8.0+ / PostgreSQL 13+
    Visual Studio 2022 或 VS Code

## 🚀 安裝與執行方式

1. 使用 Visual Studio 2022 開啟方案
2. 設定連線字串於 `appsettings.json`

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=OrderingSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

3. 套用 Migration 初始化資料庫：

```bash
dotnet ef database update
```

4. 執行專案並開始測試

