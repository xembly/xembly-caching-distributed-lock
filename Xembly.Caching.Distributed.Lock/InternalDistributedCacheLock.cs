using Microsoft.Extensions.Caching.Distributed;
using System;

namespace Xembly.Caching.Distributed.Lock
{
	internal class InternalDistributedCacheLock : IDisposable
	{
		private readonly IDistributedCache _cache;
		private readonly string _lockName;

		public InternalDistributedCacheLock(IDistributedCache cache, string name, TimeSpan timeoutAfter)
		{
			_cache = cache;
			_lockName = name;
			_cache.SetString(_lockName, DateTime.UtcNow.ToString(), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = timeoutAfter
			});
		}

		public void Dispose()
		{
			_cache.Remove(_lockName);
		}
	}
}
