using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Controllers;
using VetClinic.Api.Dtos;
using VetClinic.Api.Tests.TestHelpers;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using Xunit;

namespace VetClinic.Api.Tests.Controllers;

public class RecordatoriosControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly RecordatoriosController _controller;

    public RecordatoriosControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _controller = new RecordatoriosController(_fixture.ClinicaService);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task GetRecordatorios_RetornaRefuerzosConUrisDeWhatsAppYCorreo_CU06_RF10_RN09()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Agregar una vacuna que vence en 5 días
        var inmProxima = new Inmunizacion
        {
            PacienteId = 1,
            VeterinarioId = 1,
            TipoBiologico = TipoBiologico.Vacuna,
            NombreProducto = "Parvovirus Canino",
            FechaAplicacion = DateTime.Today.AddMonths(-11),
            FechaRefuerzo = DateTime.Today.AddDays(5)
        };
        await _fixture.ClinicaService.RegistrarInmunizacionAsync(inmProxima);

        // Act
        var result = await _controller.GetRecordatorios(30, "proximos", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IReadOnlyList<RecordatorioDto>>(okResult.Value);
        Assert.NotEmpty(dtos);

        var recordatorio = Assert.Single(dtos, r => r.NombreProducto == "Parvovirus Canino");
        Assert.Equal("Proximo", recordatorio.Estado);
        Assert.Equal(5, recordatorio.DiasDiferencia);
        Assert.Contains("https://wa.me/57", recordatorio.WhatsAppUrl);
        Assert.Contains("mailto:", recordatorio.MailtoUrl);
    }

    [Fact]
    public async Task GetRecordatorios_FiltroVencidos_RetornaRefuerzosEnMora()
    {
        // Arrange
        await _fixture.InitializeAsync();

        // Act
        var result = await _controller.GetRecordatorios(30, "vencidos", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IReadOnlyList<RecordatorioDto>>(okResult.Value);
        Assert.NotNull(dtos);
        Assert.All(dtos, r => Assert.Equal("Vencido", r.Estado));
    }
}
