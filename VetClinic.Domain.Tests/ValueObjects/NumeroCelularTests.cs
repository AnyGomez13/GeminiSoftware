using VetClinic.Domain.ValueObjects;
using Xunit;

namespace VetClinic.Domain.Tests.ValueObjects;

public class NumeroCelularTests
{
    [Theory]
    [InlineData("3001234567")]
    [InlineData("3159876543")]
    [InlineData("3201112233")]
    [InlineData("3509998877")]
    public void Crear_CelularValidoColombia_RetornaExito_RN04(string input)
    {
        // Act
        var result = NumeroCelular.Crear(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(input, result.Value.Valor);
    }

    [Theory]
    [InlineData("+573001234567", "3001234567")]
    [InlineData("573159876543", "3159876543")]
    [InlineData("300 123 4567", "3001234567")]
    [InlineData("300-123-4567", "3001234567")]
    [InlineData("(300) 123-4567", "3001234567")]
    [InlineData("300.123.4567", "3001234567")]
    [InlineData("   300   123   4567   ", "3001234567")]
    [InlineData("+57 (315) 987.6543", "3159876543")]
    public void Crear_CelularConPrefijoOEspacios_LimpiaYRetornaExito_RN04(string input, string esperado)
    {
        // Act
        var result = NumeroCelular.Crear(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value.Valor);
        Assert.Equal($"57{esperado}", result.Value.FormatoWhatsApp);
        Assert.Equal($"+57{esperado}", result.Value.NumeroInternacional);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("2001234567")] // No inicia en 3
    [InlineData("4001234567")] // No inicia en 3
    [InlineData("300123456")]  // 9 dígitos (muy corto)
    [InlineData("30012345678")] // 11 dígitos (muy largo)
    [InlineData("300123456A")] // Letras
    [InlineData("abcdefghij")]
    public void Crear_CelularInvalido_RetornaFallo_RN04(string? input)
    {
        // Act
        var result = NumeroCelular.Crear(input);

        // Assert
        Assert.True(result.IsFailure);
    }
}
