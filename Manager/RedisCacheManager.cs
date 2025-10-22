using EIU.Caching.Redis.Core;
using EIU.Caching.Redis.Helpers;

namespace EIU.Caching.Redis.Manager
{
    /// <summary>
    /// Quản lý logic cache cấp cao, có thể mở rộng cho nhiều use case phức tạp hơn
    /// </summary>
    public class RedisCacheManager : IRedisCacheManager
    {
        private readonly IRedisCacheService _cacheService;

        public RedisCacheManager(IRedisCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        /// <summary>
        /// Lấy dữ liệu cache theo key, hoặc chạy factory để cache lại nếu chưa có.
        /// Cho phép truyền TTL dạng TimeSpan hoặc string "1d2h30m".
        /// </summary>
        public async Task<T?> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            object? expiry = null
        )
        {
            // ✅ 1. Kiểm tra cache có sẵn chưa
            var data = await _cacheService.GetAsync<T>(key);
            if (data != null)
                return data;

            // ✅ 2. Gọi factory để lấy dữ liệu mới
            var result = await factory();
            if (result != null)
            {
                // ✅ 3. Chuyển đổi thời gian hết hạn
                TimeSpan? ttl = expiry switch
                {
                    null => null,
                    TimeSpan ts => ts,
                    string s => TimeSpan.FromSeconds(DurationHelper.ParseDuration(s)),
                    int seconds => TimeSpan.FromSeconds(seconds),
                    _ => null,
                };

                await _cacheService.SetAsync(key, result, ttl);
            }

            return result;
        }

        /// <summary>
        /// Xóa cache theo key
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            await _cacheService.RemoveAsync(key);
        }

        /// <summary>
        /// Xóa tất cả cache có prefix (ví dụ: "eiu:student:")
        /// </summary>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            await _cacheService.RemoveByPrefixAsync(prefix);
        }
    }
}
