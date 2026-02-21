using Microsoft.EntityFrameworkCore;
using Orm.Domain.Entities;
using Orm.Infrastructure.DbContexts;
using Orm.Infrastructure.Repositories;

namespace Orm.Infrastructure.Tests;

public class RefreshTokenRepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_PersistsRefreshToken()
    {
        // Arrange
        using var context = CreateContext(nameof(CreateAsync_PersistsRefreshToken));
        var repo = new RefreshTokenRepository(context);
        var token = new RefreshToken
        {
            Token = "test-token-123",
            UserId = "user-1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await repo.CreateAsync(token);

        // Assert
        Assert.True(result.Id > 0);
        var persisted = await context.RefreshTokens.SingleAsync();
        Assert.Equal("test-token-123", persisted.Token);
        Assert.Equal("user-1", persisted.UserId);
    }

    [Fact]
    public async Task GetByTokenAsync_ReturnsToken_WhenFound()
    {
        // Arrange
        using var context = CreateContext(nameof(GetByTokenAsync_ReturnsToken_WhenFound));
        context.RefreshTokens.Add(new RefreshToken
        {
            Token = "find-me",
            UserId = "user-1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await context.SaveChangesAsync();

        var repo = new RefreshTokenRepository(context);

        // Act
        var result = await repo.GetByTokenAsync("find-me");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("find-me", result.Token);
    }

    [Fact]
    public async Task GetByTokenAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = CreateContext(nameof(GetByTokenAsync_ReturnsNull_WhenNotFound));
        var repo = new RefreshTokenRepository(context);

        // Act
        var result = await repo.GetByTokenAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesToken()
    {
        // Arrange
        using var context = CreateContext(nameof(UpdateAsync_ModifiesToken));
        var token = new RefreshToken
        {
            Token = "update-me",
            UserId = "user-1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        var repo = new RefreshTokenRepository(context);

        // Act
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = "new-token";
        await repo.UpdateAsync(token);

        // Assert
        var persisted = await context.RefreshTokens.SingleAsync();
        Assert.NotNull(persisted.RevokedAt);
        Assert.Equal("new-token", persisted.ReplacedByToken);
    }

    [Fact]
    public async Task RevokeAllForUserAsync_RevokesAllActiveTokens()
    {
        // Arrange
        using var context = CreateContext(nameof(RevokeAllForUserAsync_RevokesAllActiveTokens));
        context.RefreshTokens.AddRange(
            new RefreshToken
            {
                Token = "token-1",
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            },
            new RefreshToken
            {
                Token = "token-2",
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            },
            new RefreshToken
            {
                Token = "token-3",
                UserId = "user-2",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            }
        );
        await context.SaveChangesAsync();

        var repo = new RefreshTokenRepository(context);

        // Act
        await repo.RevokeAllForUserAsync("user-1");

        // Assert
        var user1Tokens = await context.RefreshTokens.Where(t => t.UserId == "user-1").ToListAsync();
        Assert.All(user1Tokens, t => Assert.NotNull(t.RevokedAt));

        var user2Token = await context.RefreshTokens.SingleAsync(t => t.UserId == "user-2");
        Assert.Null(user2Token.RevokedAt);
    }
}
