# FishCash - Project Rules

## Ngôn ngữ
- Giao tiếp: **Tiếng Việt**
- Comment code / Commit message: **Tiếng Anh**

## Tech Stack
- **.NET MAUI 9** (cross-platform: Windows, MacOS, Android, iOS)
- **Entity Framework Core** + **SQLite** (local database)
- **CommunityToolkit.Mvvm** (MVVM data binding)
- Kiến trúc: **MVVM** (Model → Service → ViewModel → View)

## Cấu trúc thư mục
```
FishCash/
├── Models/          # EF Core entities (Category, Product, Order, OrderDetail, Transaction)
├── Data/            # AppDbContext, migrations
├── Services/        # Business logic (CategoryService, ProductService, OrderService, CartService, PrintService)
├── ViewModels/      # ObservableObject classes (CategoryViewModel, ProductViewModel, PosViewModel, CheckoutViewModel)
├── Views/           # XAML pages
├── Resources/       # Images, Fonts, Styles
└── Platforms/       # Platform-specific code
```

## Quy tắc Coding

### Naming Conventions
- **PascalCase**: Classes, Methods, Properties, Public fields
- **camelCase**: Local variables, parameters
- **_camelCase**: Private fields (prefix `_`)
- File name = Class name (VD: `CategoryService.cs` chứa class `CategoryService`)

### MVVM Pattern
- Mỗi ViewModel kế thừa `ObservableObject`
- Dùng `[ObservableProperty]` cho bindable properties
- Dùng `[RelayCommand]` cho ICommand bindings
- **KHÔNG** đặt business logic trong ViewModel — đặt trong Service

### Dependency Injection
- Đăng ký tất cả Services, ViewModels, Views trong `MauiProgram.cs`
- Services dùng `AddTransient<>()` hoặc `AddSingleton<>()` tùy lifecycle
- ViewModel inject Service qua constructor

### Database & Dữ liệu
- **KHÔNG BAO GIỜ** xóa hoặc thay đổi cấu trúc database mà không hỏi ý kiến user
- Luôn dùng `try-catch` cho database operations
- Migration phải được tạo và review trước khi apply

### UI/UX
- Desktop (Windows/MacOS): **Fluent UI** style
- Mobile (Android/iOS): **Material 3** style
- Nút bấm phải đủ lớn cho thao tác nhanh khi đông khách
- Thiết kế sang trọng, rõ ràng, chuyên nghiệp

### Error Handling
- Luôn handle exceptions, đặc biệt database operations và I/O
- Hiển thị thông báo lỗi thân thiện cho user (không hiện raw exception)
- Log errors trong Debug mode

### Performance
- Dùng `async/await` cho tất cả database và I/O operations
- Tránh blocking UI thread
- Tối ưu cho thiết bị tầm trung

## Áp dụng Skills

Dự án có các skills trong `.agents/skills/`. **BẮT BUỘC** đọc SKILL.md và áp dụng đúng thời điểm:

| Khi nào | Skill cần dùng | Cách áp dụng |
|---------|----------------|-------------|
| Trước khi code tính năng mới | `writing-plans` | Viết kế hoạch chi tiết, xin approval trước |
| Khi bắt tay thực thi kế hoạch | `executing-plans` | Theo đúng các bước trong plan, review checkpoint |
| Trước khi viết logic (Cart, Payment...) | `test-driven-development` | Viết test trước → code sau → refactor |
| Khi gặp bug hoặc lỗi | `systematic-debugging` | Thu thập evidence → Giả thuyết → Kiểm chứng → Fix |
| Khi hoàn thành feature/fix | `requesting-code-review` | Tạo review request, highlight thay đổi quan trọng |
| Trước khi claim "xong" | `verification-before-completion` | Chạy build, test, xác nhận output trước khi báo hoàn thành |
| Khi xử lý thanh toán, dữ liệu | `security-best-practices` | Kiểm tra bảo mật SQLite, validate input, xử lý QR an toàn |
| Khi commit code | `git-commit` | Phân tích diff, tạo conventional commit message chuẩn |
