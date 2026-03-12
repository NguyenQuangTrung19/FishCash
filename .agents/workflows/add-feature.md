---
description: Add a new feature following MVVM pattern to FishCash
---

# Add New Feature to FishCash

Follow these steps in ORDER to add a new feature. Each step depends on the previous one.

## Step 1: Model (nếu cần entity mới)
1. Tạo file `FishCash/Models/<EntityName>.cs`
2. Định nghĩa class với các properties và data annotations
3. Thêm `DbSet<EntityName>` vào `FishCash/Data/AppDbContext.cs`
4. **HỎI USER** trước khi thay đổi database schema

## Step 2: Service
1. Tạo file `FishCash/Services/<FeatureName>Service.cs`
2. Inject `AppDbContext` qua constructor
3. Viết các methods async: GetAll, GetById, Create, Update, Delete
4. Luôn dùng `try-catch` cho database operations

## Step 3: ViewModel
1. Tạo file `FishCash/ViewModels/<FeatureName>ViewModel.cs`
2. Kế thừa `ObservableObject`
3. Inject Service qua constructor
4. Dùng `[ObservableProperty]` cho data binding
5. Dùng `[RelayCommand]` cho commands (Load, Save, Delete...)

## Step 4: View (XAML Page)
1. Tạo file `FishCash/Views/<FeatureName>Page.xaml` và `.xaml.cs`
2. Set `BindingContext` trong constructor code-behind
3. Bind UI elements tới ViewModel properties và commands
4. Desktop: dùng Fluent UI style | Mobile: dùng Material 3 style

## Step 5: Register DI
1. Mở `FishCash/MauiProgram.cs`
2. Đăng ký Service: `builder.Services.AddTransient<FeatureNameService>();`
3. Đăng ký ViewModel: `builder.Services.AddTransient<FeatureNameViewModel>();`
4. Đăng ký View: `builder.Services.AddTransient<FeatureNamePage>();`

## Step 6: Navigation
1. Mở `FishCash/AppShell.xaml`
2. Thêm ShellContent hoặc route mới cho page
3. Nếu cần, đăng ký route trong `AppShell.xaml.cs`: `Routing.RegisterRoute(nameof(FeatureNamePage), typeof(FeatureNamePage));`

## Step 7: Build & Verify
// turbo
1. Build to verify no compile errors:
```
dotnet build FishCash/FishCash.csproj -f net9.0-windows10.0.19041.0
```
2. Kiểm tra navigation hoạt động
3. Kiểm tra CRUD operations hoạt động đúng
