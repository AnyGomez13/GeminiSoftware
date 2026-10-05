using System.Diagnostics;
using System.Text.RegularExpressions;
using VetClinic.Domain.Common;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Infrastructure.Services;

public class ExternalLauncherService : IExternalLauncherService
{
    private static readonly Regex CelularRegex = new(@"^3[0-9]{9}$", RegexOptions.Compiled);
    private readonly Action<string> _processLauncher;

    public ExternalLauncherService(Action<string>? processLauncher = null)
    {
        _processLauncher = processLauncher ?? LaunchViaShell;
    }

    public static string ConstruirUriWhatsApp(string telefono, string mensaje)
    {
        var limpio = telefono.Trim().Replace(" ", "").Replace("-", "");
        if (limpio.StartsWith("+57"))
        {
            limpio = limpio[3..];
        }
        else if (limpio.StartsWith("57") && limpio.Length == 12)
        {
            limpio = limpio[2..];
        }

        if (!CelularRegex.IsMatch(limpio))
        {
            throw new ArgumentException("El número debe contener exactamente 10 dígitos numéricos iniciando con 3.", nameof(telefono));
        }

        var mensajeCodificado = Uri.EscapeDataString(mensaje);
        return $"https://wa.me/57{limpio}?text={mensajeCodificado}";
    }

    public static string ConstruirUriCorreo(string email, string asunto, string cuerpo)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("El correo electrónico del propietario no es válido.", nameof(email));
        }

        var asuntoCodificado = Uri.EscapeDataString(asunto);
        var cuerpoCodificado = Uri.EscapeDataString(cuerpo);
        return $"mailto:{email.Trim()}?subject={asuntoCodificado}&body={cuerpoCodificado}";
    }

    public Result AbrirWhatsApp(string telefono, string mensaje)
    {
        try
        {
            var uri = ConstruirUriWhatsApp(telefono, mensaje);
            _processLauncher(uri);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"No se pudo abrir el enlace de WhatsApp: {ex.Message}");
        }
    }

    public Result AbrirCorreo(string email, string asunto, string cuerpo)
    {
        try
        {
            var uri = ConstruirUriCorreo(email, asunto, cuerpo);
            _processLauncher(uri);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"No se pudo abrir el cliente de correo: {ex.Message}");
        }
    }

    private static void LaunchViaShell(string uri)
    {
        var psi = new ProcessStartInfo(uri)
        {
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
