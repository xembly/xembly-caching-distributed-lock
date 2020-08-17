using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System;
using Xunit;

namespace Xembly.Caching.Distributed.Lock.Tests
{
	public class InternalLockTests
	{
		private readonly IFixture _fixture;

		public InternalLockTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
		}

		[Fact]
		public void CanSetAndReleaseLock()
		{
			const string name = "internal:test";
			var timeoutAfter = TimeSpan.FromSeconds(2);
			var cache = _fixture.Create<Mock<IDistributedCache>>();
			var createdCheck = false;
			var removedCheck = false;

			cache
				.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()))
				.Callback<string, byte[], DistributedCacheEntryOptions>((key, value, options) =>
				{
					key.Should().Be("internal:test");
					var dt = System.Text.Encoding.Default.GetString(value);
					DateTime.Parse(dt).Should().BeCloseTo(DateTime.UtcNow, 1000);
					options.AbsoluteExpirationRelativeToNow.Should().Be(timeoutAfter);
					createdCheck = true;
				});

			cache
				.Setup(x => x.Remove(It.IsAny<string>()))
				.Callback<string>(key =>
				{
					key.Should().Be("internal:test");
					removedCheck = true;
				});

			var sut = new InternalDistributedCacheLock(cache.Object, name, timeoutAfter);
			createdCheck.Should().BeTrue();
			removedCheck.Should().BeFalse();

			sut.Dispose();
			removedCheck.Should().BeTrue();
		}
	}
}
