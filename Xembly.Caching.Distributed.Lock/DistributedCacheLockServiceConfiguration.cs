using Microsoft.Extensions.DependencyInjection;
using System;

namespace Xembly.Caching.Distributed.Lock
{
	public static class DistributedCacheLockServiceConfiguration
	{
		public static IServiceCollection AddDistributedCacheLock(this IServiceCollection services)
		{
			return services.AddDistributedCacheLock(_ => { });
		}

		public static IServiceCollection AddDistributedCacheLock(this IServiceCollection services, Action<DistributedCacheLockOptions> setupAction)
		{
			services.Configure(setupAction);
			return services.AddSingleton<IDistributedCacheLock, DistributedCacheLock>();
		}
	}
}
