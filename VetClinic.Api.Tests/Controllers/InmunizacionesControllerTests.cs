using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using VetClinic.Domain.Enums;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class InmunizacionesControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly InmunizacionesController _controller;

    public InmunizacionesControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new InmunizacionesController(_fixture.ClinicaService, _fixture.PdfExportService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task RegistrarInmunizacion_DatosValidos_Retorna201Created_RF08_CU05()
    {
        // Arrange
        await _fixture.InitializeAsync();
        var dto = new CrearInmunizacionDto(
            1, // Paciente semilla ID 1
            1, // Dr. Fabio
            TipoBiologico.Vacuna,
            "Rabia Canina Nobivac",
            "LOTE-2026-X",
            DateTime.Today,
            DateTime.Today.AddYears(1),
            "Aplicación sin novedad"
        );

        // Act
        var result = await _controller.RegistrarInmunizacion(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<InmunizacionDto>(createdResult.Value);
        Assert.Equal("Rabia Canina Nobivac", response.NombreProducto);
        Assert.True(response.Id > 0);
    }

    [Fact]
    public async Task DescargarCarnetPdf_PacienteExistente_RetornaPdfEnMemoria_RF09_RNF06()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act (Paciente semilla ID 1)
        var result = await _controller.DescargarCarnetPdf(1, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.NotEmpty(fileResult.FileContents);
        Assert.Contains(".pdf", fileResult.FileDownloadName);
    }
}
