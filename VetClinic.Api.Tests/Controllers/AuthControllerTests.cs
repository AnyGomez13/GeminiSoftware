using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new AuthController(_fixture.AuthService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task Login_CredencialesCorrectas_Retorna200OkConToken_RF01_CU01()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var request = new LoginRequestDto("admin", "Clinica2026*");

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Usuario);
        Assert.Equal("admin", response.Usuario.Username);
        Assert.NotNull(response.Token);
    }

    [Fact]
    public async Task Login_ContrasenaInvalida_Retorna401Unauthorized_RF01()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var request = new LoginRequestDto("admin", "ContrasenaErronea123");

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<LoginResponseDto>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Null(response.Usuario);
    }

    [Fact]
    public async Task Login_CamposVacios_Retorna400BadRequest()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var request = new LoginRequestDto("", "");

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<LoginResponseDto>(badRequestResult.Value);
        Assert.False(response.Success);
    }
}
