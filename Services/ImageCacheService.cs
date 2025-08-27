// Services/ImageCacheService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SphericalImageViewer.Services
{
    public class ImageCacheService
    {
        private readonly ConcurrentDictionary<string, BitmapSource> _cache = new ConcurrentDictionary<string, BitmapSource>();
        private readonly ConcurrentDictionary<string, DateTime> _cacheTimestamps = new ConcurrentDictionary<string, DateTime>();
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
        private readonly int _maxCacheSize = 50;

        public void ClearCache()
        {
            _cache.Clear();
            _cacheTimestamps.Clear();
        }

        public void ClearExpiredItems()
        {
            var now = DateTime.Now;
            var expiredKeys = new List<string>();

            foreach (var kvp in _cacheTimestamps)
            {
                if (now - kvp.Value > _cacheExpiry)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
                _cacheTimestamps.TryRemove(key, out _);
            }
        }

        public BitmapSource GetCachedImage(string key)
        {
            if (_cache.TryGetValue(key, out var image))
            {
                // Update timestamp
                _cacheTimestamps.TryUpdate(key, DateTime.Now, _cacheTimestamps[key]);
                return image;
            }
            return null;
        }

        public void CacheImage(string key, BitmapSource image)
        {
            // Remove oldest items if cache is full
            if (_cache.Count >= _maxCacheSize)
            {
                var oldestKey = "";
                var oldestTime = DateTime.MaxValue;

                foreach (var kvp in _cacheTimestamps)
                {
                    if (kvp.Value < oldestTime)
                    {
                        oldestTime = kvp.Value;
                        oldestKey = kvp.Key;
                    }
                }

                if (!string.IsNullOrEmpty(oldestKey))
                {
                    _cache.TryRemove(oldestKey, out _);
                    _cacheTimestamps.TryRemove(oldestKey, out _);
                }
            }

            _cache.TryAdd(key, image);
            _cacheTimestamps.TryAdd(key, DateTime.Now);
        }

        public string GenerateCacheKey(string imagePath, double yaw, double pitch, double roll, double fov)
        {
            return $"{imagePath}_{yaw:F1}_{pitch:F1}_{roll:F1}_{fov:F1}";
        }
    }
}

