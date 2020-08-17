using Microsoft.Extensions.Logging;
using System;

namespace Xembly.Caching.Distributed.Lock
{
	public class DistributedCacheLockOptions
	{
		public LogLevel AbandonedTaskLogLevel { get; set; } = LogLevel.Warning;
		public TimeSpan DefaultTimeoutAfter { get; set; } = TimeSpan.FromSeconds(10);
		public string KeyPrefix { get; set; } = "distributed:cache:entry:";
	}
}
