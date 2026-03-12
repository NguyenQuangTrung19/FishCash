---
description: Run FishCash MAUI app in debug mode
---

# Run FishCash (Debug)

## Windows Desktop
// turbo
1. Build and run the app on Windows:
```
dotnet build FishCash/FishCash.csproj -f net9.0-windows10.0.19041.0 -t:Run
```

## Lưu ý
- App sẽ tự mở sau khi build xong
- Debug logs sẽ hiển thị trong Output window
- Để dừng app, đóng cửa sổ ứng dụng hoặc Ctrl+C trong terminal
- Database SQLite sẽ được tạo tự động ở thư mục AppData khi lần đầu chạy
