using FluentAssertions;
using QuotesApi.Infrastructure;
using QuotesApi.Services;

namespace Quotes.Tests.Unit.Services;

public class RefreshTokenServiceTests
{
    [Fact]
    public void IsReuseDetected_RevokedTokenWithReplacement_ReturnsTrue()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        var revokedAt = clock.UtcNow;
        var replacedByToken = "replacement-token";

        var result = service.IsReuseDetected(revokedAt, replacedByToken);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsReuseDetected_ActiveToken_ReturnsFalse()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        DateTimeOffset? revokedAt = null;
        var replacedByToken = "replacement-token";

        var result = service.IsReuseDetected(revokedAt, replacedByToken);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsReuseDetected_RevokedTokenWithoutReplacement_ReturnsFalse()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        var revokedAt = clock.UtcNow;
        string? replacedByToken = null;

        var result = service.IsReuseDetected(revokedAt, replacedByToken);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsReuseDetected_ActiveTokenWithoutReplacement_ReturnsFalse()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        DateTimeOffset? revokedAt = null;
        string? replacedByToken = null;

        var result = service.IsReuseDetected(revokedAt, replacedByToken);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ExpirationBeforeCurrentTime_ReturnsTrue()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        var expiresAt = clock.UtcNow.AddMinutes(-1);

        var result = service.IsExpired(expiresAt);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ExpirationAtCurrentTime_ReturnsTrue()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        var expiresAt = clock.UtcNow;

        var result = service.IsExpired(expiresAt);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ExpirationAfterCurrentTime_ReturnsFalse()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        };
        var service = new RefreshTokenService(clock);
        var expiresAt = clock.UtcNow.AddMinutes(1);

        var result = service.IsExpired(expiresAt);

        result.Should().BeFalse();
    }
    
}