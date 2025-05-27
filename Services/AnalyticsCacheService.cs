using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseHR.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace WiseHR.Services
{
    public class AnalyticsCacheService
    {
        private readonly IMemoryCache _cache;
        private const string ANALYTICS_CACHE_KEY = "analytics_data";
        private const string LOOKUP_CACHE_KEY_PREFIX = "analytics_lookup_";
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(30);

        public AnalyticsCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<EmployeeAnalyticsResponseDto>> GetOrSetAnalyticsDataAsync(Func<Task<List<EmployeeAnalyticsResponseDto>>> dataFactory)
        {
            if (!_cache.TryGetValue(ANALYTICS_CACHE_KEY, out List<EmployeeAnalyticsResponseDto> cachedData))
            {
                cachedData = await dataFactory();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_defaultCacheDuration)
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(ANALYTICS_CACHE_KEY, cachedData, cacheEntryOptions);
            }

            return cachedData;
        }

        public async Task<List<string>> GetOrSetLookupDataAsync(string lookupType, Func<Task<List<string>>> dataFactory)
        {
            string cacheKey = $"{LOOKUP_CACHE_KEY_PREFIX}{lookupType}";

            if (!_cache.TryGetValue(cacheKey, out List<string> cachedData))
            {
                cachedData = await dataFactory();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_defaultCacheDuration)
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, cachedData, cacheEntryOptions);
            }

            return cachedData;
        }

        public void ClearAnalyticsCache()
        {
            _cache.Remove(ANALYTICS_CACHE_KEY);
        }

        public void ClearLookupCache(string lookupType)
        {
            string cacheKey = $"{LOOKUP_CACHE_KEY_PREFIX}{lookupType}";
            _cache.Remove(cacheKey);
        }

        public void ClearAllLookupCache()
        {
            _cache.Remove(ANALYTICS_CACHE_KEY);
            _cache.Remove($"{LOOKUP_CACHE_KEY_PREFIX}nationalities");
            _cache.Remove($"{LOOKUP_CACHE_KEY_PREFIX}designations");
            _cache.Remove($"{LOOKUP_CACHE_KEY_PREFIX}joiningLocations");
            _cache.Remove($"{LOOKUP_CACHE_KEY_PREFIX}levels");
        }

        public void ClearAllCache()
        {
            ClearAnalyticsCache();
            ClearAllLookupCache();
        }
    }
} 