using System.Globalization;
using VetClinic.Domain.Common;

namespace VetClinic.Domain.ValueObjects;

public sealed record PesoCorporal
{
    public const double MinimoKg = 0.01;
    public const double MaximoKg = 150.00;

    public double Valor { get; }

    private PesoCorporal(double valor)
    {
        Valor = Math.Round(valor, 2);
    }

    public static Result<PesoCorporal> Crear(double valor)
    {
        if (valor < MinimoKg || valor > MaximoKg)
        {
            return Result.Failure<PesoCorporal>(
                $"El peso corporal debe estar estrictamente en kilogramos (Kg), entre {MinimoKg} y {MaximoKg} Kg.");
        }

        return Result.Success(new PesoCorporal(valor));
    }

    public override string ToString() => $"{Valor.ToString("F2", CultureInfo.InvariantCulture)} Kg";

    public static implicit operator double(PesoCorporal peso) => peso.Valor;
}
