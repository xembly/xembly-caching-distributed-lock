using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Xembly.Caching.Distributed.Lock.Tests
{
	public class LockTests
	{
		private readonly IFixture _fixture;

		public LockTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
		}

		[Fact]
		public async Task CanSetLock()
		{
			var cache = _fixture.Create<Mock<IDistributedCache>>();
			var logger = _fixture.Create<Mock<ILogger<DistributedCacheLock>>>();
			var options = _fixture.Create<Mock<IOptions<DistributedCacheLockOptions>>>();

			cache
				.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(() => Task.FromResult<byte[]>(null));

			cache
				.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()))
				.Callback<string, byte[], DistributedCacheEntryOptions>((key, value, opts) =>
				{
					key.Should().Be("distributed:cache:entry:test");
					var dt = System.Text.Encoding.Default.GetString(value);
					DateTime.Parse(dt).Should().BeCloseTo(DateTime.UtcNow, 1000);
					opts.AbsoluteExpirationRelativeToNow.Should().Be(options.Object.Value.DefaultTimeoutAfter);
				});

			cache
				.Setup(x => x.Remove(It.IsAny<string>()))
				.Callback<string>(key => key.Should().Be("distributed:cache:entry:test"));

			options
				.SetupGet(x => x.Value)
				.Returns(new DistributedCacheLockOptions());

			const string name = "test";
			var sut = new DistributedCacheLock(cache.Object, logger.Object, options.Object);
			var locker = await sut.Acquire(name);
			locker.Should().NotBeNull();
			locker.Dispose();
		}

		[Fact]
		public async Task FailSetLock()
		{
			var cache = _fixture.Create<Mock<IDistributedCache>>();
			var logger = _fixture.Create<Mock<ILogger<DistributedCacheLock>>>();
			var options = _fixture.Create<Mock<IOptions<DistributedCacheLockOptions>>>();

			cache
				.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(() => Task.FromResult(System.Text.Encoding.Default.GetBytes("value")));

			options
				.SetupGet(x => x.Value)
				.Returns(new DistributedCacheLockOptions { DefaultTimeoutAfter = TimeSpan.FromMilliseconds(10) });

			const string name = "test";
			var sut = new DistributedCacheLock(cache.Object, logger.Object, options.Object);
			Func<Task<IDisposable>> act = async () => await sut.Acquire(name);
			await act.Should().ThrowAsync<DistributedCacheLockAcquireException>();
		}
	}
}
