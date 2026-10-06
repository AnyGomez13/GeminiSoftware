using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using VetClinic.Api.Dtos;

namespace VetClinic.Api.Tests.Integration;

public class WebApiE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public WebApiE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ServesIndexHtmlOFallback_Retorna200Ok()
    {
        // RNF-07, RNF-08: Kestrel sirve la SPA local
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSwagger_Retorna200Ok()
    {
        var response = await _client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthLogin_ConCredencialesValidas_RetornaExitoYUsuario()
    {
        // CU-01, RF-01, RN-01: Login con PBKDF2
        var payload = new LoginRequestDto("admin", "Clinica2026*");
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Usuario);
        Assert.Equal("admin", result.Usuario.Username);
    }

    [Fact]
    public async Task AuthLogin_ConContrasenaErronea_RetornaUnauthorized()
    {
        // RN-01: Credenciales inválidas bloquean acceso
        var payload = new LoginRequestDto("admin", "ContrasenaEquivocada");
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetVeterinarios_RetornaDresFabioYWilliam()
    {
        // RN-02, STF-01: Trazabilidad nominal veterinaria
        var response = await _client.GetAsync("/api/veterinarios");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vets = await response.Content.ReadFromJsonAsync<List<VeterinarioDto>>(_jsonOptions);
        Assert.NotNull(vets);
        Assert.Contains(vets, v => v.Nombre.Contains("Fabio"));
        Assert.Contains(vets, v => v.Nombre.Contains("William"));
    }

    [Fact]
    public async Task GetRecordatorios_RetornaEnlacesWhatsAppCeroCosto()
    {
        // RF-10, RN-09, RNF-09: Enlace directo wa.me sin pasarelas de pago
        var response = await _client.GetAsync("/api/recordatorios?dias=365&estado=todos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var recordatorios = await response.Content.ReadFromJsonAsync<List<RecordatorioDto>>(_jsonOptions);
        Assert.NotNull(recordatorios);
        if (recordatorios.Count > 0)
        {
            Assert.All(recordatorios, r =>
            {
                Assert.StartsWith("https://wa.me/57", r.WhatsAppUrl);
            });
        }
    }

    [Fact]
    public async Task GetCarnetPdf_RetornaContenidoPdfBinario()
    {
        // RF-09, RNF-06: Streaming de Carnet PDF en < 3s
        // Se asume paciente ID 1 registrado por semilla (Maya)
        var response = await _client.GetAsync("/api/inmunizaciones/carnet-pdf/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
        // Validar firma cabecera PDF (%PDF-)
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }
}
