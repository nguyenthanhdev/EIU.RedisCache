# 🧱 Hướng dẫn build và publish NuGet – EIU.Caching.Redis

## 1️⃣ Chuẩn bị môi trường
- .NET SDK 8.0+
- Redis Server hoặc Redis docker
- RedisInsight (tùy chọn)

Kiểm tra:
```bash
dotnet --version
```

## 2️⃣ Build thư viện
```bash
dotnet clean
dotnet build -c Release
```

## 3️⃣ Đóng gói NuGet
```bash
dotnet pack -c Release
```
Output: `bin/Release/EIU.Caching.Redis.<version>.nupkg`
```
