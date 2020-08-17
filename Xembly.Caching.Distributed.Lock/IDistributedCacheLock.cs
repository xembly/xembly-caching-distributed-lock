using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xembly.Caching.Distributed.Lock
{
	public interface IDistributedCacheLock
	{
		Task<IDisposable> Acquire(string name, CancellationToken cancellationToken = default);
		Task<IDisposable> Acquire(string name, TimeSpan timeoutAfter, CancellationToken cancellationToken = default);
	}
}
