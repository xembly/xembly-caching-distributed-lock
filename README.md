# Xembly Distributed Cache Lock

![Build Xembly.Caching.Distributed.Lock](https://github.com/xembly/xembly-caching-distributed-lock/workflows/Build%20Xembly.Caching.Distributed.Lock/badge.svg)

Distributed Cache Lock (DCL) is an addon for Microsoft's Distributed Cache library. DCL utilizies this library
to create a distributed lock for whatever type of distributed cache system you use with the Distributed Cache
Library provided by Microsoft.

## Installing via NuGet

    Install-Package Xembly.Caching.Distributed.Lock

## Setup

```csharp
services.AddDistributedCacheLock();
```

or with options

```csharp
services.AddDistributedCacheLock(options =>
{
    options.AbandonedTaskLogLevel = LogLevel.Warning;
    options.DefaultTimeoutAfter = TimeSpan.FromSeconds(10);
    options.KeyPrefix = "distributed:cache:entry:";
});
```

## Usage

```csharp
public class MyService
{
	private readonly IDistributedCacheLock _lock;

	public MyService(IDistributedCacheLock dcl)
	{
		_lock = dcl;
	}

	public async Task Run(CancellationToken ct)
	{
		// Create the lock with default timeout
		using (await _lock.Acquire("nameOfLock", ct))
		{
			// do something while there is a lock
		}

		// Create the lock with custom timeout
		using (await _lock.Acquire("nameOfLock", TimeSpan.FromSeconds(30), ct))
		{
			// do something while there is a lock
		}
	}
}
```

## License

Licensed under the terms of the [BSD-3-Clause License](https://opensource.org/licenses/BSD-3-Clause)

## Credits

This application uses Open Source components. You can find the source code of their open source projects along with license information below. We acknowledge and are grateful to these developers for their contributions to open source.

Project: [Microsoft.Extensions.Caching.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Abstractions/)<br>
Copyright © Microsoft Corporation. All rights reserved.<br>
License [Apache-2.0](https://licenses.nuget.org/Apache-2.0)

Project: [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/)<br>
Copyright © Microsoft Corporation. All rights reserved.<br>
License [Apache-2.0](https://licenses.nuget.org/Apache-2.0)

Project: [Microsoft.Extensions.Options](https://www.nuget.org/packages/Microsoft.Extensions.Options/)<br>
Copyright © Microsoft Corporation. All rights reserved.<br>
License [Apache-2.0](https://licenses.nuget.org/Apache-2.0)

Project: [Polly](https://www.nuget.org/packages/Polly/)<br>
Copyright © 2020, App vNext<br>
License [BSD-3-Clause](https://licenses.nuget.org/BSD-3-Clause)
