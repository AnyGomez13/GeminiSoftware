using VetClinic.Domain.Enums;

namespace VetClinic.Api.Dtos;

public record RecordatorioDto(
    int InmunizacionId,
    int PacienteId,
    string NombrePaciente,
    string EspeciePaciente,
    int PropietarioId,
    string NombrePropietario,
    string TelefonoPropietario,
    string? EmailPropietario,
    TipoBiologico TipoBiologico,
    string NombreProducto,
    DateTime FechaAplicacion,
    DateTime FechaRefuerzo,
    string Estado, // "Proximo" | "Vencido"
    int DiasDiferencia, // Días faltantes (positivo) o días de retraso (negativo)
    string MensajeSugerido,
    string WhatsAppUrl,
    string? MailtoUrl
);
