using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class AtencionesControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AtencionesController _controller;

    public AtencionesControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new AtencionesController(_fixture.ClinicaService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task RegistrarAtencion_DatosValidos_Retorna201YSincronizaPesoPaciente_RF06_RN02_RN05()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var dto = new CrearAtencionDto(
            1, // Paciente semilla ID 1
            1, // Dr. Fabio
            DateTime.Now,
            32.4, // Nuevo peso registrado en consulta
            "Control de rutina y vacunación",
            "Constantes vitales normales, mucosas rosadas",
            "Paciente en óptimas condiciones",
            "Plan de desparasitación al día",
            "Mantener hidratación",
            DateTime.Today.AddMonths(6)
        );

        // Act
        var result = await _controller.RegistrarAtencion(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<AtencionClinicaDto>(createdResult.Value);
        Assert.Equal(32.4, response.PesoConsultaKg);
        Assert.True(response.Id > 0);

        // Verificar sincronización automática del peso del paciente (RN-05, RN-06)
        var pacienteActualizado = await _fixture.ClinicaService.ObtenerPacientePorIdAsync(1);
        Assert.NotNull(pacienteActualizado);
        Assert.Equal(32.4, pacienteActualizado.PesoActualKg);
    }

    [Fact]
    public async Task RegistrarAtencion_SinVeterinarioTratante_Retorna400BadRequest_RN02()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var dto = new CrearAtencionDto(
            1,
            0, // Sin veterinario obligatorio
            DateTime.Now,
            15.0,
            "Consulta general",
            null,
            "Diagnóstico",
            "Tratamiento",
            null,
            null
        );

        // Act
        var result = await _controller.RegistrarAtencion(dto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetHistorialPorPaciente_RetornaHistorialCronologico_RF07_RN07()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act
        var result = await _controller.GetHistorialPorPaciente(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsAssignableFrom<IReadOnlyList<AtencionClinicaDto>>(okResult.Value);
        Assert.NotNull(lista);
    }
}
