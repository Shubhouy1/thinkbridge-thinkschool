using FluentAssertions;
using QuotesApi.Infrastructure;
using QuotesApi.Services;

namespace Quotes.Tests.Integration;

public class RefreshTokenServiceTests
{
    private readonly FakeClock _clock = new()
    {
        UtcNow = new DateTimeOffset(
            2026, 8, 13, 10, 0, 0, TimeSpan.Zero)
    };

    private RefreshTokenService CreateService()
    {
        return new RefreshTokenService(_clock);
    }

    [Fact]
    public void IsReuseDetected_WhenTokenWasRevokedAndReplaced_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.IsReuseDetected(
            _clock.UtcNow,
            "replacement-token");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsReuseDetected_WhenRevokedAtIsNull_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.IsReuseDetected(
            null,
            "replacement-token");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsReuseDetected_WhenReplacementTokenIsNull_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.IsReuseDetected(
            _clock.UtcNow,
            null);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiryIsBeforeCurrentTime_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.IsExpired(
            _clock.UtcNow.AddMinutes(-1));

        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiryEqualsCurrentTime_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.IsExpired(_clock.UtcNow);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiryIsAfterCurrentTime_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.IsExpired(
            _clock.UtcNow.AddMinutes(1));

        result.Should().BeFalse();
    }
}