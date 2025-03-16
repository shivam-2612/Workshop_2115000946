using StackExchange.Redis;
using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Repository_Layer.Service
{
    public class RedisCacheService
    {
        private readonly IDatabase _cache;
        private readonly TimeSpan _cacheExpiry;

        public RedisCacheService(IConfiguration configuration)
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
                _cache = redis.GetDatabase();
                _cacheExpiry = TimeSpan.FromMinutes(Convert.ToDouble(configuration["Redis:CacheExpiryMinutes"]));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Redis Connection Error: {ex.Message}");
                throw new Exception("❌ Unable to connect to Redis. Check if Redis is running.");
            }
        }

        public void Set<T>(string key, T value)
        {
            var jsonData = JsonSerializer.Serialize(value);
            _cache.StringSet(key, jsonData, _cacheExpiry);
        }

        public T? Get<T>(string key)
        {
            var jsonData = _cache.StringGet(key);
            return jsonData.HasValue ? JsonSerializer.Deserialize<T>(jsonData) : default;
        }

        public void Remove(string key)
        {
            _cache.KeyDelete(key);
        }
    }
}
