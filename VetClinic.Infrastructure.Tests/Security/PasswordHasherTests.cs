using VetClinic.Infrastructure.Security;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_GeneraHashYSaltNoVacios_RNF01_STF02()
    {
        // Act
        var hash = _hasher.HashPassword("PasswordSeguro123*", out var salt);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.False(string.IsNullOrWhiteSpace(salt));
    }

    [Fact]
    public void VerifyPassword_CredencialCorrecta_RetornaTrue_RN01_RNF01()
    {
        // Arrange
        const string password = "MiPasswordClinica2026*";
        var hash = _hasher.HashPassword(password, out var salt);

        // Act
        var esValido = _hasher.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(esValido);
    }

    [Fact]
    public void VerifyPassword_CredencialIncorrecta_RetornaFalse_RN01()
    {
        // Arrange
        var hash = _hasher.HashPassword("PasswordCorrecto", out var salt);

        // Act
        var esValido = _hasher.VerifyPassword("PasswordIncorrecto", hash, salt);

        // Assert
        Assert.False(esValido);
    }

    [Fact]
    public void VerifyPassword_ClaveSemillaAdmin_VerificaExitosamente_RN01()
    {
        // Arrange
        const string passwordSemilla = "Clinica2026*";
        var hash = _hasher.HashPassword(passwordSemilla, out var salt);

        // Act
        var esValido = _hasher.VerifyPassword(passwordSemilla, hash, salt);

        // Assert
        Assert.True(esValido);
    }
}
