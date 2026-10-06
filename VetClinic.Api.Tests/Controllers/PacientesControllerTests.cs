using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using VetClinic.Domain.Enums;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class PacientesControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly PacientesController _controller;

    public PacientesControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new PacientesController(_fixture.ClinicaService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task CrearPaciente_DatosValidos_Retorna201Created_RF04_RN06()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var dto = new CrearPacienteDto(
            1, // Propietario existente de semilla
            "Max",
            Especie.Canino,
            "Labrador",
            Sexo.Macho,
            DateTime.Today.AddYears(-2),
            false,
            25.5,
            "Dorado",
            EstadoReproductivo.Entero
        );

        // Act
        var result = await _controller.CrearPaciente(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<PacienteDto>(createdResult.Value);
        Assert.Equal("Max", response.Nombre);
        Assert.True(response.Id > 0);
        Assert.Contains("2 años", response.EdadFormateada);
    }

    [Fact]
    public async Task GetById_PacienteExistente_RetornaDetalleCompletoConCurvaPeso_CU03_CU04()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act (Paciente semilla ID 1)
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var detalle = Assert.IsType<PacienteDetalleDto>(okResult.Value);
        Assert.Equal(1, detalle.Id);
        Assert.NotNull(detalle.Propietario);
        Assert.NotEmpty(detalle.CurvaPeso);
        Assert.Equal("Registro Inicial", detalle.CurvaPeso[0].TipoEvento);
    }

    [Fact]
    public async Task GetById_PacienteInexistente_Retorna404NotFound()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act
        var result = await _controller.GetById(9999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
