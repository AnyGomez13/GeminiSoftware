using System.Collections.ObjectModel;
using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Domain.ValueObjects;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class NuevaAtencionViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;

    private int _pacienteId;
    private string _pacienteNombre = string.Empty;
    private ObservableCollection<Veterinario> _veterinarios = new();
    private Veterinario? _veterinarioSeleccionado;
    private DateTime _fechaHoraAtencion = DateTime.Now;
    private double _pesoConsultaKg = 5.0;
    private string _motivoConsulta = string.Empty;
    private string? _examenClinico;
    private string _diagnostico = string.Empty;
    private string _tratamiento = string.Empty;
    private DateTime? _fechaControl;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isGuardado;

    public NuevaAtencionViewModel(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;

        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        CancelarCommand = new RelayCommand(Cancelar);

        _ = CargarVeterinariosAsync();
    }

    public int PacienteId
    {
        get => _pacienteId;
        set => SetProperty(ref _pacienteId, value);
    }

    public string PacienteNombre
    {
        get => _pacienteNombre;
        set => SetProperty(ref _pacienteNombre, value);
    }

    public ObservableCollection<Veterinario> Veterinarios
    {
        get => _veterinarios;
        set => SetProperty(ref _veterinarios, value);
    }

    public Veterinario? VeterinarioSeleccionado
    {
        get => _veterinarioSeleccionado;
        set
        {
            if (SetProperty(ref _veterinarioSeleccionado, value))
            {
                LimpiarError();
            }
        }
    }

    public DateTime FechaHoraAtencion
    {
        get => _fechaHoraAtencion;
        set => SetProperty(ref _fechaHoraAtencion, value);
    }

    public double PesoConsultaKg
    {
        get => _pesoConsultaKg;
        set => SetProperty(ref _pesoConsultaKg, value);
    }

    public string MotivoConsulta
    {
        get => _motivoConsulta;
        set
        {
            if (SetProperty(ref _motivoConsulta, value))
            {
                LimpiarError();
            }
        }
    }

    public string? ExamenClinico
    {
        get => _examenClinico;
        set => SetProperty(ref _examenClinico, value);
    }

    public string Diagnostico
    {
        get => _diagnostico;
        set
        {
            if (SetProperty(ref _diagnostico, value))
            {
                LimpiarError();
            }
        }
    }

    public string Tratamiento
    {
        get => _tratamiento;
        set
        {
            if (SetProperty(ref _tratamiento, value))
            {
                LimpiarError();
            }
        }
    }

    public DateTime? FechaControl
    {
        get => _fechaControl;
        set => SetProperty(ref _fechaControl, value);
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

    public AtencionClinica? AtencionResult { get; private set; }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event Action? RequestClose;

    public async Task CargarVeterinariosAsync()
    {
        try
        {
            var lista = await _clinicaService.ObtenerVeterinariosActivosAsync();
            Veterinarios = new ObservableCollection<Veterinario>(lista);
            VeterinarioSeleccionado = Veterinarios.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MostrarError($"Error cargando lista de veterinarios: {ex.Message}");
        }
    }

    public void ConfigurarPaciente(Paciente paciente)
    {
        PacienteId = paciente.Id;
        PacienteNombre = $"{paciente.Nombre} ({paciente.Especie} - {paciente.Raza})";
        PesoConsultaKg = paciente.PesoActualKg > 0 ? paciente.PesoActualKg : 5.0;
        FechaHoraAtencion = DateTime.Now;
        MotivoConsulta = string.Empty;
        ExamenClinico = string.Empty;
        Diagnostico = string.Empty;
        Tratamiento = string.Empty;
        FechaControl = null;
        LimpiarError();
    }

    public async Task GuardarAsync()
    {
        if (IsBusy) return;

        // Validación nominal obligatoria del veterinario (RN-02, STF-01)
        if (VeterinarioSeleccionado == null || VeterinarioSeleccionado.Id <= 0)
        {
            MostrarError("Debe seleccionar obligatoriamente al Médico Veterinario tratante (Dr. Fabio o Dr. William).");
            return;
        }

        if (string.IsNullOrWhiteSpace(MotivoConsulta))
        {
            MostrarError("El motivo de consulta (anamnesis) es obligatorio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Diagnostico))
        {
            MostrarError("El diagnóstico clínico o presunción es obligatorio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Tratamiento))
        {
            MostrarError("El plan de tratamiento y fórmula médica son obligatorios.");
            return;
        }

        var pesoValidation = PesoCorporal.Crear(PesoConsultaKg);
        if (!pesoValidation.IsSuccess)
        {
            MostrarError(pesoValidation.Error.Message);
            return;
        }

        IsBusy = true;
        LimpiarError();

        try
        {
            var atencion = new AtencionClinica
            {
                PacienteId = PacienteId,
                VeterinarioId = VeterinarioSeleccionado.Id,
                FechaHoraAtencion = FechaHoraAtencion,
                PesoConsultaKg = pesoValidation.Value.Valor,
                MotivoConsulta = MotivoConsulta.Trim(),
                ExamenClinico = ExamenClinico?.Trim(),
                Diagnostico = Diagnostico.Trim(),
                Tratamiento = Tratamiento.Trim(),
                FechaControl = FechaControl
            };

            var resultado = await _clinicaService.RegistrarAtencionAsync(atencion);
            if (resultado.IsSuccess)
            {
                AtencionResult = resultado.Value;
                IsGuardado = true;
                RequestClose?.Invoke();
            }
            else
            {
                MostrarError(resultado.Error.Message);
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error al guardar atención médica: {ex.Message}");
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
