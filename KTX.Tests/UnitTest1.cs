using FluentAssertions;
using KTX.Data;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class AuthServiceTests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public AuthServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<KtxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUsernameOrPasswordIsEmpty()
    {
        // Arrange
        var mockFactory = new Mock<IDbContextFactory<KtxDbContext>>();
        var service = new AuthService(mockFactory.Object);

        // Act
        var result = await service.LoginAsync(new LoginRequest { Username = "", Password = "" });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Vui lòng nhập đầy đủ");
    }

    [Fact]
    public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreValid()
    {
        // Arrange
        var rawPassword = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        using (var db = new KtxDbContext(_dbOptions))
        {
            var role = new VaiTro { Id = 1, TenVaiTro = "Admin" };
            var user = new NguoiDung
            {
                Id = 1,
                TenDangNhap = "testadmin",
                MatKhauHash = hashedPassword,
                HoTen = "Test Administrator",
                MaVaiTro = 1,
                VaiTro = role,
                HoatDong = true
            };
            db.VaiTros.Add(role);
            db.NguoiDungs.Add(user);
            await db.SaveChangesAsync();
        }

        var mockFactory = new Mock<IDbContextFactory<KtxDbContext>>();
        mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new KtxDbContext(_dbOptions));

        var service = new AuthService(mockFactory.Object);

        // Act
        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "testadmin",
            Password = rawPassword
        });

        // Assert
        result.Success.Should().BeTrue();
        result.Session.Should().NotBeNull();
        result.Session!.Username.Should().Be("testadmin");
        result.Session.RoleName.Should().Be("Admin");
    }
}
