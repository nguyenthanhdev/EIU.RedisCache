namespace EIU.Caching.Redis.Manager
{
    /// <summary>
    /// Interface quản lý cache ở tầng logic nghiệp vụ
    /// </summary>
    public interface IRedisCacheManager
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, object? expiry = null);

        Task RemoveAsync(string key);

        Task RemoveByPrefixAsync(string prefix);
    }
}
