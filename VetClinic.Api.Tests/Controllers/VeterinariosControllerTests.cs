using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class VeterinariosControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly VeterinariosController _controller;

    public VeterinariosControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new VeterinariosController(_fixture.ClinicaService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task GetVeterinariosActivos_RetornaListaConDresFabioYWilliam_RN02_STF01()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act
        var result = await _controller.GetVeterinariosActivos(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsAssignableFrom<IReadOnlyList<VeterinarioDto>>(okResult.Value);
        Assert.NotEmpty(lista);
        Assert.Contains(lista, v => v.Nombre.Contains("Fabio"));
        Assert.Contains(lista, v => v.Nombre.Contains("William"));
    }
}
