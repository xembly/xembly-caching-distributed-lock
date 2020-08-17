using System;

namespace Xembly.Caching.Distributed.Lock
{
	public class DistributedCacheLockAcquireException : Exception
	{
		public DistributedCacheLockAcquireException()
		{ }

		public DistributedCacheLockAcquireException(string message, Exception innerException) : base(message, innerException)
		{ }
	}
}
