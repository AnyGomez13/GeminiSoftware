using VetClinic.Domain.ValueObjects;
using Xunit;

namespace VetClinic.Domain.Tests.ValueObjects;

public class PesoCorporalTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(1.5)]
    [InlineData(15.25)]
    [InlineData(45.0)]
    [InlineData(150.00)]
    public void Crear_PesoEnRangoValido_RetornaExito_RN06(double input)
    {
        // Act
        var result = PesoCorporal.Crear(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(input, result.Value.Valor);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.01)]
    [InlineData(-10.5)]
    [InlineData(150.01)]
    [InlineData(250.0)]
    public void Crear_PesoFueraDeRango_RetornaFallo_RN06(double input)
    {
        // Act
        var result = PesoCorporal.Crear(input);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToString_FormatoEstandarKg_RetornaSufijoKg_RN06()
    {
        // Arrange
        var result = PesoCorporal.Crear(12.5);

        // Act & Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("12.50 Kg", result.Value.ToString());
    }
}
