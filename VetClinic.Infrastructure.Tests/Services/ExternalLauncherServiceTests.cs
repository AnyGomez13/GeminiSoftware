using VetClinic.Infrastructure.Services;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Services;

public class ExternalLauncherServiceTests
{
    [Fact]
    public void ConstruirUriWhatsApp_ParametrosValidos_GeneraUriCorrecta_RF10_RN09()
    {
        // Arrange
        const string telefono = "3001234567";
        const string mensaje = "Hola Carlos, la Clínica Veterinaria le recuerda que su mascota Rocky tiene vacuna el 15/10/2026.";

        // Act
        var uri = ExternalLauncherService.ConstruirUriWhatsApp(telefono, mensaje);

        // Assert
        Assert.StartsWith("https://wa.me/573001234567?text=", uri);
        Assert.Contains("Hola%20Carlos", uri);
        Assert.Contains("15%2F10%2F2026", uri);
    }

    [Theory]
    [InlineData("+573001234567", "573001234567")]
    [InlineData("573001234567", "573001234567")]
    [InlineData("300 123 4567", "573001234567")]
    [InlineData("300-123-4567", "573001234567")]
    public void ConstruirUriWhatsApp_FormatoConPrefijoOEspacios_NormalizaPrefijoNacional(string entrada, string prefijoEsperado)
    {
        // Act
        var uri = ExternalLauncherService.ConstruirUriWhatsApp(entrada, "Mensaje de prueba");

        // Assert
        Assert.StartsWith($"https://wa.me/{prefijoEsperado}?text=", uri);
    }

    [Theory]
    [InlineData("2001234567")] // No inicia en 3
    [InlineData("300123456")]  // Menos de 10 dígitos
    [InlineData("30012345678")] // Más de 10 dígitos
    [InlineData("abcdefghij")]
    public void ConstruirUriWhatsApp_TelefonoInvalido_ArrojaArgumentException(string telefonoInvalido)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExternalLauncherService.ConstruirUriWhatsApp(telefonoInvalido, "Mensaje"));
    }

    [Fact]
    public void ConstruirUriCorreo_ParametrosValidos_GeneraUriMailtoCorrecta_RF11_RN09()
    {
        // Arrange
        const string email = "cliente@correo.com";
        const string asunto = "Recordatorio Médico - Rocky";
        const string cuerpo = "Estimado cliente, su mascota tiene cita médica.";

        // Act
        var uri = ExternalLauncherService.ConstruirUriCorreo(email, asunto, cuerpo);

        // Assert
        Assert.StartsWith("mailto:cliente@correo.com?subject=", uri);
        Assert.Contains("Recordatorio%20M%C3%A9dico", uri);
        Assert.Contains("&body=Estimado%20cliente", uri);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("correo_sin_arroba.com")]
    public void ConstruirUriCorreo_EmailInvalido_ArrojaArgumentException(string emailInvalido)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ExternalLauncherService.ConstruirUriCorreo(emailInvalido, "Asunto", "Cuerpo"));
    }

    [Fact]
    public void AbrirWhatsApp_ParametrosValidos_EjecutaLauncherConUri_RetornaExito()
    {
        // Arrange
        string? uriCapturada = null;
        var servicio = new ExternalLauncherService(uri => uriCapturada = uri);

        // Act
        var resultado = servicio.AbrirWhatsApp("3159998877", "Recordatorio");

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(uriCapturada);
        Assert.StartsWith("https://wa.me/573159998877?text=Recordatorio", uriCapturada);
    }

    [Fact]
    public void AbrirCorreo_ParametrosValidos_EjecutaLauncherConUri_RetornaExito()
    {
        // Arrange
        string? uriCapturada = null;
        var servicio = new ExternalLauncherService(uri => uriCapturada = uri);

        // Act
        var resultado = servicio.AbrirCorreo("propietario@test.com", "Cita", "Cuerpo");

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(uriCapturada);
        Assert.StartsWith("mailto:propietario@test.com?subject=Cita&body=Cuerpo", uriCapturada);
    }

    [Fact]
    public void AbrirWhatsApp_TelefonoInvalido_RetornaFalloSinLanzarExcepcionNoControlada()
    {
        // Arrange
        var servicio = new ExternalLauncherService(_ => { });

        // Act
        var resultado = servicio.AbrirWhatsApp("12345", "Mensaje");

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Contains("No se pudo abrir el enlace de WhatsApp", resultado.Error.Message);
    }
}
