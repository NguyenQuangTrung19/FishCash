---
description: Build FishCash MAUI app for Windows
---

# Build FishCash

## Windows Desktop
// turbo
1. Build the project for Windows:
```
dotnet build FishCash/FishCash.csproj -f net9.0-windows10.0.19041.0
```

## Android (nếu cần)
1. Build the project for Android:
```
dotnet build FishCash/FishCash.csproj -f net9.0-android
```

## Lưu ý
- Đảm bảo đã cài .NET 9 SDK
- Nếu build Windows lỗi, kiểm tra Windows SDK version trong `.csproj`
- Nếu build Android lỗi, kiểm tra Android SDK đã được cài qua Visual Studio
