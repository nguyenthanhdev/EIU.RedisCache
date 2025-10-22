using EIU.Infrastructure.Redis.Core;
using EIU.Infrastructure.Redis.Interceptors;
using EIU.Infrastructure.Redis.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EIU.Infrastructure.Redis.Extensions
{
  /// <summary>
  /// Tiện ích mở rộng giúp cấu hình và đăng ký Redis Cache vào Dependency Injection container.
  /// Hỗ trợ đọc cấu hình từ appsettings.json (section mặc định: "RedisCache").
  /// </summary>
  public static class RedisServiceCollectionExtensions
  {
    /// <summary>
    /// 🧩 Đăng ký toàn bộ Redis caching components:
    /// - RedisCacheOptions (từ appsettings)
    /// - RedisCacheService, RedisCacheManager, RedisCacheInterceptor
    /// </summary>
    /// <param name="services">IServiceCollection trong Startup hoặc Program.cs</param>
    /// <param name="configuration">Cấu hình appsettings.json hoặc môi trường</param>
    /// <returns>IServiceCollection để chain tiếp cấu hình</returns>
    public static IServiceCollection AddRedisCaching(
                this IServiceCollection services,
                IConfigurationSection redisSection)
    {
            //  Đọc cấu hình Redis
            var redisConfig = redisSection.Get<RedisCacheOptions>() ?? new RedisCacheOptions();

            //  Kiểm tra hợp lệ cấu hình
            if (!redisConfig.Enabled)
            {
              Console.WriteLine("⚠️ RedisCacheConfig.Enabled = false → Bộ nhớ đệm Redis sẽ không được kích hoạt.");
              return services;
            }

            if (string.IsNullOrWhiteSpace(redisConfig.ConnectionString))
            {
              throw new InvalidOperationException("❌ Cấu hình RedisCacheConfig.ConnectionString bị thiếu hoặc rỗng. Vui lòng kiểm tra lại file appsettings.json!");
            }

            //  Đăng ký Redis cấu hình vào DI
            services.AddSingleton<IOptions<RedisCacheOptions>>(_ => Options.Create(redisConfig));
            services.AddSingleton(redisConfig);

            // 🧩 Khởi tạo kết nối Redis ngay lập tức
            IConnectionMultiplexer? redisConnection = null;
            try
            {
                Console.WriteLine($"🔌 Kết nối Redis: {redisConfig.ConnectionString}");
                redisConnection = ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
                Console.WriteLine("✅ Kết nối Redis thành công!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ Không thể kết nối Redis: {ex.Message}");
                Console.ResetColor();
            }

            // 🔗 Nếu kết nối được thì đăng ký bình thường
            if (redisConnection != null && redisConnection.IsConnected)
            {
                services.AddSingleton<IConnectionMultiplexer>(redisConnection);
                services.AddSingleton<IRedisCacheService, RedisCacheService>();
                services.AddSingleton<IRedisCacheManager, RedisCacheManager>();
                services.AddSingleton<RedisCacheInterceptor>();
                Console.WriteLine("🚀 Redis caching đã được kích hoạt!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Redis không khả dụng → RedisCacheService sẽ không được đăng ký.");
                Console.ResetColor();
            }
            return services;
        }
      }
}
