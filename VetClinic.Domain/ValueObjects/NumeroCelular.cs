using System.Text.RegularExpressions;
using VetClinic.Domain.Common;

namespace VetClinic.Domain.ValueObjects;

public sealed record NumeroCelular
{
    private static readonly Regex CelularColombiaRegex = new(@"^3[0-9]{9}$", RegexOptions.Compiled);

    public string Valor { get; }
    public string FormatoWhatsApp => $"57{Valor}";
    public string NumeroInternacional => $"+57{Valor}";

    private NumeroCelular(string valor)
    {
        Valor = valor;
    }

    public static Result<NumeroCelular> Crear(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result.Failure<NumeroCelular>("El número de celular no puede estar vacío.");
        }

        var limpio = input.Trim().Replace(" ", "").Replace("-", "");
        if (limpio.StartsWith("+57"))
        {
            limpio = limpio[3..];
        }
        else if (limpio.StartsWith("57") && limpio.Length == 12)
        {
            limpio = limpio[2..];
        }

        if (!CelularColombiaRegex.IsMatch(limpio))
        {
            return Result.Failure<NumeroCelular>(
                "El número celular debe ser válido para Colombia: 10 dígitos numéricos iniciando con 3.");
        }

        return Result.Success(new NumeroCelular(limpio));
    }

    public override string ToString() => Valor;

    public static implicit operator string(NumeroCelular celular) => celular.Valor;
}
