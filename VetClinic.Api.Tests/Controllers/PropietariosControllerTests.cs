using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using VetClinic.Domain.Enums;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class PropietariosControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly PropietariosController _controller;

    public PropietariosControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new PropietariosController(_fixture.ClinicaService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task CrearPropietario_CelularColombiaValido_Retorna201Created_RF02_RN04()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var dto = new CrearPropietarioDto(
            TipoDocumento.CC,
            "1098765432",
            "Carlos",
            "Mendoza",
            "3001234567",
            "carlos.mendoza@email.com",
            "Calle 10 # 20-30"
        );

        // Act
        var result = await _controller.CrearPropietario(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<PropietarioDto>(createdResult.Value);
        Assert.Equal("Carlos Mendoza", response.NombreCompleto);
        Assert.Equal("3001234567", response.Telefono);
        Assert.True(response.Id > 0);
    }

    [Fact]
    public async Task CrearPropietario_CelularInvalido_Retorna400BadRequest_RN04()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var dto = new CrearPropietarioDto(
            TipoDocumento.CC,
            "1098765433",
            "Laura",
            "Gómez",
            "2001234567", // No empieza por 3
            "laura@email.com",
            "Carrera 5 # 12-34"
        );

        // Act
        var result = await _controller.CrearPropietario(dto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task BuscarPropietarios_FiltroPorDocumento_RetornaResultados()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act
        var result = await _controller.BuscarPropietarios("1098765432", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsAssignableFrom<IReadOnlyList<PropietarioDto>>(okResult.Value);
        Assert.NotNull(lista);
    }
}
