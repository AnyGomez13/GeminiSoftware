using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Security;
using VetClinic.Infrastructure.Services;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VetClinicDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VetClinicDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VetClinicDbContext(options);
        _passwordHasher = new PasswordHasher();
        _authService = new AuthService(_context, _passwordHasher);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AuthenticateAsync_CredencialesValidas_RetornaExitoYUsuario_RF01_RN01_CU01()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        // Act
        var result = await _authService.AuthenticateAsync("admin", "Clinica2026*");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("admin", result.Value.Username);
        Assert.Equal("Administrador de Estación", result.Value.NombreCompleto);
    }

    [Fact]
    public async Task AuthenticateAsync_PasswordInvalido_RetornaFallo_RF01_RN01()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        // Act
        var result = await _authService.AuthenticateAsync("admin", "PasswordErroneo123");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Usuario o contraseña incorrectos.", result.Error.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_UsuarioInexistente_RetornaFallo_RF01()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        // Act
        var result = await _authService.AuthenticateAsync("noexiste", "Clinica2026*");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Usuario o contraseña incorrectos.", result.Error.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_CamposVacios_RetornaFallo_RF01()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        // Act
        var resultVacio = await _authService.AuthenticateAsync("", "");
        var resultEspacios = await _authService.AuthenticateAsync("   ", "   ");

        // Assert
        Assert.False(resultVacio.IsSuccess);
        Assert.False(resultEspacios.IsSuccess);
        Assert.Equal("Debe ingresar usuario y contraseña.", resultVacio.Error.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_UsuarioInactivo_RetornaFallo_RN01()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var admin = await _context.Usuarios.FirstAsync(u => u.Username == "admin");
        admin.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("admin", "Clinica2026*");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Usuario o contraseña incorrectos.", result.Error.Message);
    }
}
