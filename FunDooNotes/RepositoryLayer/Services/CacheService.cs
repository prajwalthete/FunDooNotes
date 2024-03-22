using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace RepositoryLayer.Services
{
    public class CacheService : IDistributedCache
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _cache;

        public CacheService(string connectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            _cache = _connectionMultiplexer.GetDatabase();
        }

        public byte[]? Get(string key)
        {
            return _cache.StringGet(key);
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return await _cache.StringGetAsync(key);
        }

        public void Refresh(string key)
        {
            // Redis does not support refresh, so just ignore this operation
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            // Redis does not support refresh, so just ignore this operation
            await Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _cache.KeyDelete(key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await _cache.KeyDeleteAsync(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var expiry = options?.AbsoluteExpirationRelativeToNow;
            if (expiry.HasValue)
            {
                _cache.StringSet(key, value, expiry);
            }
            else
            {
                _cache.StringSet(key, value);
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var expiry = options?.AbsoluteExpirationRelativeToNow;
            if (expiry.HasValue)
            {
                await _cache.StringSetAsync(key, value, expiry);
            }
            else
            {
                await _cache.StringSetAsync(key, value);
            }
        }


        public bool TryGetValue(string key, out byte[] value)
        {
            var redisValue = _cache.StringGet(key);
            if (redisValue.HasValue)
            {
                value = redisValue;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

    }
}