using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Xembly.Caching.Distributed.Lock.Tests
{
	public class OptionTests
	{
		[Fact]
		public void DefaultOptions()
		{
			var sut = new DistributedCacheLockOptions();
			sut.AbandonedTaskLogLevel.Should().Be(LogLevel.Warning);
			sut.DefaultTimeoutAfter.Should().Be(TimeSpan.FromSeconds(10));
			sut.KeyPrefix.Should().Be("distributed:cache:entry:");
		}

		[Fact]
		public void CustomOptions()
		{
			var sut = new DistributedCacheLockOptions
			{
				AbandonedTaskLogLevel = LogLevel.Error,
				DefaultTimeoutAfter = TimeSpan.FromMinutes(1),
				KeyPrefix = "app:cache:"
			};
			sut.AbandonedTaskLogLevel.Should().Be(LogLevel.Error);
			sut.DefaultTimeoutAfter.Should().Be(TimeSpan.FromMinutes(1));
			sut.KeyPrefix.Should().Be("app:cache:");
		}
	}
}
