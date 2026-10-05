using VetClinic.Domain.Common;
using VetClinic.Domain.Entities;

namespace VetClinic.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<Result<Usuario>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
