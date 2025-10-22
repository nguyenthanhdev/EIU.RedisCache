# 🧩 Tích hợp EIU.Caching.Redis vào project khác

## 1️⃣ Thêm package NuGet
```bash
dotnet add package EIU.Caching.Redis
```

## 2️⃣ Cấu hình trong `appsettings.json`
```json
"RedisCacheConfig": {
  "ConnectionString": "localhost:6379,user=[USERNAME],password=[PASSWORD],defaultDatabase=[DB INDEX]",
  "ProjectAlias": "HR",
  "Enabled": true,
  "DefaultDuration": "15m"
}
```

## 3️⃣ Khởi tạo trong `Program.cs`
```csharp
builder.Services.AddRedisCaching(builder.Configuration.GetSection("RedisCacheConfig"));
```

## 4️⃣ Sử dụng
Xem chi tiết tại README.md (controller, service, manual caching)

## 5️⃣ Kiểm tra bằng RedisInsight
Mở `http://localhost:5540`, kết nối Redis và xem key có prefix `HR:`.
