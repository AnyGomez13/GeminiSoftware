using VetClinic.Domain.Common;

namespace VetClinic.Domain.Interfaces.Services;

public interface IExternalLauncherService
{
    Result AbrirWhatsApp(string telefono, string mensaje);
    Result AbrirCorreo(string email, string asunto, string cuerpo);
}
