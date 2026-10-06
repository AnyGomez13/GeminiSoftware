using VetClinic.Domain.Common;

namespace VetClinic.Domain.Interfaces.Services;

public interface IPdfExportService
{
    Task<Result<string>> GenerarCarnetVacunacionAsync(int pacienteId, string rutaDestino, CancellationToken cancellationToken = default);
    Task<Result<byte[]>> GenerarCarnetVacunacionBytesAsync(int pacienteId, CancellationToken cancellationToken = default);
}
