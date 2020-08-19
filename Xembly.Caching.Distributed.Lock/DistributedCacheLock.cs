using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xembly.Caching.Distributed.Lock
{
	public class DistributedCacheLock : IDistributedCacheLock
	{
		private readonly IDistributedCache _cache;
		private readonly ILogger _logger;
		private readonly DistributedCacheLockOptions _options;
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		public DistributedCacheLock(IDistributedCache cache, ILogger<DistributedCacheLock> logger, IOptions<DistributedCacheLockOptions> options)
		{
			_cache = cache;
			_logger = logger;
			_options = options.Value;
		}

		public Task<IDisposable> Acquire(string name, CancellationToken cancellationToken = default)
		{
			return Acquire(name, _options.DefaultTimeoutAfter, cancellationToken);
		}

		public async Task<IDisposable> Acquire(string name, TimeSpan timeoutAfter, CancellationToken cancellationToken = default)
		{
			var retry = Policy
				.Handle<DistributedCacheLockAcquireException>()
				.RetryForeverAsync();  // TODO :: add option to configure wait times betweeen retries
			var timeout = Policy.TimeoutAsync(timeoutAfter, TimeoutStrategy.Pessimistic, AbandonedTask);
			var policies = timeout.WrapAsync(retry);

			var result = await policies.ExecuteAndCaptureAsync(async ct =>
			{
				var key = _options.KeyPrefix + name;
				var data = string.Empty;
				try
				{
					await _semaphore.WaitAsync(cancellationToken);
					if (!ct.IsCancellationRequested)
						data = await _cache.GetStringAsync(key);
					if (!string.IsNullOrEmpty(data)) throw new DistributedCacheLockAcquireException();
					return new InternalDistributedCacheLock(_cache, key, timeoutAfter);
				}
				finally
				{
					_semaphore.Release();
				}
			}, cancellationToken);

			if (result.FinalException != null)
			{
				_logger.LogError($"Failed to acquire cache lock '{name}'", result.FinalException);
				throw new DistributedCacheLockAcquireException($"Failed to acquire cache lock '{name}'", result.FinalException);
			}
			_logger.LogDebug($"Acquired cache lock '{name}' at {DateTime.Now}");
			return result.Result;
		}

		private Task AbandonedTask(Context _, TimeSpan __, Task abandonedTask)
		{
			abandonedTask.ContinueWith(t =>
			{
				if (t.IsFaulted) _logger.LogWarning($"The task previously walked-away-from now terminated with exception: {t.Exception.Message}");
				else if (t.IsCanceled) _logger.LogWarning("The task previously walked-away-from now was canceled.");
				else _logger.LogWarning("The task previously walked-away-from now eventually completed.");
			});
			return Task.CompletedTask;
		}
	}
}
