using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Domain.ValueObjects;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class PropietarioModalViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;
    private int _id;
    private TipoDocumento _tipoDocumento = TipoDocumento.CC;
    private string _numeroDocumento = string.Empty;
    private string _nombres = string.Empty;
    private string _apellidos = string.Empty;
    private string _telefono = string.Empty;
    private string? _direccion;
    private string? _email;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isGuardado;

    public PropietarioModalViewModel(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;
        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public TipoDocumento TipoDocumento
    {
        get => _tipoDocumento;
        set => SetProperty(ref _tipoDocumento, value);
    }

    public string NumeroDocumento
    {
        get => _numeroDocumento;
        set
        {
            if (SetProperty(ref _numeroDocumento, value))
            {
                LimpiarError();
            }
        }
    }

    public string Nombres
    {
        get => _nombres;
        set
        {
            if (SetProperty(ref _nombres, value))
            {
                LimpiarError();
            }
        }
    }

    public string Apellidos
    {
        get => _apellidos;
        set
        {
            if (SetProperty(ref _apellidos, value))
            {
                LimpiarError();
            }
        }
    }

    public string Telefono
    {
        get => _telefono;
        set
        {
            if (SetProperty(ref _telefono, value))
            {
                LimpiarError();
            }
        }
    }

    public string? Direccion
    {
        get => _direccion;
        set => SetProperty(ref _direccion, value);
    }

    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
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

    public bool IsGuardado
    {
        get => _isGuardado;
        private set => SetProperty(ref _isGuardado, value);
    }

    public Propietario? PropietarioResult { get; private set; }

    public bool EsEdicion => Id > 0;
    public string TituloVentana => EsEdicion ? "Modificar Propietario" : "Registrar Nuevo Propietario";

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event Action? RequestClose;

    public void CargarParaEdicion(Propietario propietario)
    {
        Id = propietario.Id;
        TipoDocumento = propietario.TipoDocumento;
        NumeroDocumento = propietario.NumeroDocumento;
        Nombres = propietario.Nombres;
        Apellidos = propietario.Apellidos;
        Telefono = propietario.Telefono;
        Direccion = propietario.Direccion;
        Email = propietario.Email;
        LimpiarError();
        OnPropertyChanged(nameof(EsEdicion));
        OnPropertyChanged(nameof(TituloVentana));
    }

    public async Task GuardarAsync()
    {
        if (IsBusy) return;

        // Validaciones previas de formulario (RF-02, RN-04, RN-10)
        if (string.IsNullOrWhiteSpace(NumeroDocumento))
        {
            MostrarError("El número de documento es obligatorio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Nombres) || string.IsNullOrWhiteSpace(Apellidos))
        {
            MostrarError("Los nombres y apellidos son obligatorios.");
            return;
        }

        var celularValidation = NumeroCelular.Crear(Telefono);
        if (!celularValidation.IsSuccess)
        {
            MostrarError(celularValidation.Error.Message);
            return;
        }

        IsBusy = true;
        LimpiarError();

        try
        {
            if (Id == 0)
            {
                var nuevo = new Propietario
                {
                    TipoDocumento = TipoDocumento,
                    NumeroDocumento = NumeroDocumento.Trim(),
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Telefono = celularValidation.Value.Valor,
                    Direccion = Direccion?.Trim(),
                    Email = Email?.Trim()
                };

                var resultado = await _clinicaService.CrearPropietarioAsync(nuevo);
                if (resultado.IsSuccess)
                {
                    PropietarioResult = resultado.Value;
                    IsGuardado = true;
                    RequestClose?.Invoke();
                }
                else
                {
                    MostrarError(resultado.Error.Message);
                }
            }
            else
            {
                var entidad = await _clinicaService.ObtenerPropietarioPorIdAsync(Id);
                if (entidad == null)
                {
                    MostrarError("El propietario a actualizar no fue encontrado.");
                    return;
                }

                entidad.TipoDocumento = TipoDocumento;
                entidad.NumeroDocumento = NumeroDocumento.Trim();
                entidad.Nombres = Nombres.Trim();
                entidad.Apellidos = Apellidos.Trim();
                entidad.Telefono = celularValidation.Value.Valor;
                entidad.Direccion = Direccion?.Trim();
                entidad.Email = Email?.Trim();

                var resultado = await _clinicaService.ActualizarPropietarioAsync(entidad);
                if (resultado.IsSuccess)
                {
                    PropietarioResult = resultado.Value;
                    IsGuardado = true;
                    RequestClose?.Invoke();
                }
                else
                {
                    MostrarError(resultado.Error.Message);
                }
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error al guardar propietario: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Cancelar()
    {
        IsGuardado = false;
        RequestClose?.Invoke();
    }

    private void MostrarError(string mensaje)
    {
        ErrorMessage = mensaje;
        HasError = true;
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
