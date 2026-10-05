using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using Xunit;

namespace VetClinic.Domain.Tests.Entities;

public class PacienteTests
{
    [Fact]
    public void CalcularEdadFormateada_NeonatoMenor30Dias_RetornaDias_RN05_STF06()
    {
        // Arrange
        var fechaNacimiento = new DateTime(2026, 9, 25);
        var fechaReferencia = new DateTime(2026, 10, 5); // 10 días
        var paciente = new Paciente { FechaNacimiento = fechaNacimiento };

        // Act
        var edad = paciente.CalcularEdadFormateada(fechaReferencia);

        // Assert
        Assert.Equal("10 días", edad);
    }

    [Fact]
    public void CalcularEdadFormateada_UnDiaDeNacido_RetornaUnDia_RN05_STF06()
    {
        // Arrange
        var fechaNacimiento = new DateTime(2026, 10, 4);
        var fechaReferencia = new DateTime(2026, 10, 5);
        var paciente = new Paciente { FechaNacimiento = fechaNacimiento };

        // Act
        var edad = paciente.CalcularEdadFormateada(fechaReferencia);

        // Assert
        Assert.Equal("1 día", edad);
    }

    [Fact]
    public void CalcularEdadFormateada_Menor12Meses_RetornaMesesYDias_RN05_STF06()
    {
        // Arrange: 3 meses y 15 días
        var fechaNacimiento = new DateTime(2026, 6, 20);
        var fechaReferencia = new DateTime(2026, 10, 5);
        var paciente = new Paciente { FechaNacimiento = fechaNacimiento };

        // Act
        var edad = paciente.CalcularEdadFormateada(fechaReferencia);

        // Assert
        Assert.Equal("3 meses, 15 días", edad);
    }

    [Fact]
    public void CalcularEdadFormateada_MayorOIgualUnAnio_RetornaAniosYMeses_RN05_STF06()
    {
        // Arrange: 2 años y 3 meses
        var fechaNacimiento = new DateTime(2024, 7, 5);
        var fechaReferencia = new DateTime(2026, 10, 5);
        var paciente = new Paciente { FechaNacimiento = fechaNacimiento };

        // Act
        var edad = paciente.CalcularEdadFormateada(fechaReferencia);

        // Assert
        Assert.Equal("2 años, 3 meses", edad);
    }

    [Fact]
    public void CalcularEdadFormateada_ExactamenteUnAnio_RetornaUnAnio_RN05_STF06()
    {
        // Arrange
        var fechaNacimiento = new DateTime(2025, 10, 5);
        var fechaReferencia = new DateTime(2026, 10, 5);
        var paciente = new Paciente { FechaNacimiento = fechaNacimiento };

        // Act
        var edad = paciente.CalcularEdadFormateada(fechaReferencia);

        // Assert
        Assert.Equal("1 año", edad);
    }

    [Fact]
    public void CalcularEdadFormateada_FechaFutura_RetornaCeroDias_RN05()
    {
        // Arrange
        var fechaNacimiento = new DateTime(2026, 10, 10);
        var fechaReferencia = new DateTime(2026, 10, 5);
        var paciente = new Paciente { FechaNacimiento = fechaNacimiento };

        // Act
        var edad = paciente.CalcularEdadFormateada(fechaReferencia);

        // Assert
        Assert.Equal("0 días", edad);
    }
}
