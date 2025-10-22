# 🧩 EIU.Caching.Redis

**EIU.Caching.Redis** là thư viện Redis caching nội bộ do EIU phát triển,
cung cấp giải pháp cache thống nhất cho các dịch vụ .NET (API, Service, Background Worker,...).

---

## 🚀 Tính năng chính

- Tích hợp Redis làm **Distributed Cache / Memory Sharing**
- Hỗ trợ:
  - `[RedisCache]` – Cache tự động cho Action hoặc Service method
  - `[RedisCacheRemove]` – Tự động xoá cache liên quan khi dữ liệu thay đổi
  - `IRedisCacheManager` – API cấp service cho việc cache thủ công
- TTL động hỗ trợ dạng `"1d2h30m"`
- Hỗ trợ prefix `ProjectAlias` để phân tách cache giữa các hệ thống dùng chung Redis
- Sẵn sàng đóng gói làm **NuGet nội bộ**

---

## 🧱 Cấu trúc thư mục

```
EIU.Caching.Redis
├── Attributes/
├── Core/
├── Extensions/
├── Helper/
└── Manager/
```

---

## ⚙️ Cấu hình Redis

```json
"RedisCacheConfig": {
  "ConnectionString": "localhost:6379,user=[USERNAME]password=[PASSWORD],defaultDatabase=[DB INDEX]",
  "ProjectAlias": "HR",
  "Enabled": true,
  "DefaultDuration": "1d"
}
```

---

## 🧩 Cách khởi tạo

```csharp
using EIU.Caching.Redis.Extensions;

builder.Services.AddRedisCaching(builder.Configuration.GetSection("RedisCacheConfig"));
```

---

## 💡 Sử dụng

### Caching tại Controller

```csharp
[HttpGet("{id}")]
[RedisCache("5m")]
public async Task<IActionResult> GetStudent(Guid id)
{
    return Ok(await _service.GetByIdAsync(id));
}

[HttpPut]
[RedisCacheRemove("student:getlist", "student:detail:{id}")]
public async Task<IActionResult> Update(StudentVM dto)
{
    ...
}
```

### Caching tại Service

```csharp
[RedisMethodCache("student:getlist", "10m")]
public async Task<IEnumerable<StudentVM>> GetAllAsync() => await _repo.GetAll();

[RedisMethodCacheRemove("student:getlist", "student:detail:{0}")]
public async Task<bool> UpdateAsync(StudentVM dto) => await _repo.Update(dto);
```

### Cache thủ công

```csharp
public class StudentService
{
    private readonly IRedisCacheManager _cache;

    public StudentService(IRedisCacheManager cache) => _cache = cache;

    public async Task<IEnumerable<StudentVM>> GetAllAsync()
        => await _cache.GetOrSetAsync("hr:student:getall", () => _repo.GetAll(), "15m");
}
```

---

## 🧰 Môi trường Redis (docker-compose mẫu)

```yaml
version: "3.8"
services:
  redis:
    image: redis:7.4.4-alpine
    container_name: redis-server
    restart: always
    ports:
      - "6379:6379"
    volumes:
      - /home/soft-ware/redis/data:/data
      - /home/soft-ware/redis/users.acl:/usr/local/etc/redis/users.acl
    command: ["redis-server", "--appendonly", "yes", "--aclfile", "/usr/local/etc/redis/users.acl"]
    networks:
      - web-net

  redis-insight:
    image: redis/redisinsight:latest
    container_name: redis-insight
    restart: always
    ports:
      - "5540:5540"
    depends_on:
      - redis
    volumes:
      - /home/soft-ware/redis-insight/data:/data
    networks:
      - web-net
```

---

## 🧾 Giấy phép & tác giả

- **Tác giả:** EIU Software Engineering Team
- **Bản quyền:** © 2025 EIU Corporation
- **License:** Proprietary (nội bộ)
