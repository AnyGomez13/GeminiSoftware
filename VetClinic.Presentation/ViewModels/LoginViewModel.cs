using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new AsyncRelayCommand(EjecutarLoginAsync);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                LimpiarError();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                LimpiarError();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public ICommand LoginCommand { get; }

    public event Action<Usuario>? LoginSucceeded;

    public async Task EjecutarLoginAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        LimpiarError();

        try
        {
            var resultado = await _authService.AuthenticateAsync(Username, Password);
            if (resultado.IsSuccess)
            {
                LoginSucceeded?.Invoke(resultado.Value);
            }
            else
            {
                ErrorMessage = resultado.Error.Message;
                HasError = true;
                Password = string.Empty; // Limpia la contraseña ante error (CU-01)
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error inesperado de autenticación: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LimpiarError()
    {
        if (HasError)
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }
    }
}
