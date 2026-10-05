using System.Collections.ObjectModel;
using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Presentation.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class HistoriaClinicaViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    private Paciente? _pacienteActual;
    private ObservableCollection<AtencionClinica> _historialAtenciones = new();
    private ObservableCollection<Paciente> _pacientesDisponibles = new();
    private string _criterioBusquedaPaciente = string.Empty;

    public HistoriaClinicaViewModel(
        IClinicaService clinicaService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clinicaService = clinicaService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        NuevaConsultaCommand = new RelayCommand(AbrirNuevaConsulta);
        IrAVacunacionCommand = new RelayCommand(NavegarAVacunacion);
        VolverCommand = new RelayCommand(VolverAPacientes);
        BuscarPacienteCommand = new AsyncRelayCommand(BuscarPacientesAsync);

        _ = InicializarAsync();
    }

    public Paciente? PacienteActual
    {
        get => _pacienteActual;
        set
        {
            if (SetProperty(ref _pacienteActual, value))
            {
                OnPropertyChanged(nameof(TienePacienteSeleccionado));
                if (value != null)
                {
                    _ = CargarHistorialAsync(value.Id);
                }
                else
                {
                    HistorialAtenciones = new ObservableCollection<AtencionClinica>();
                }
            }
        }
    }

    public bool TienePacienteSeleccionado => PacienteActual != null;

    public ObservableCollection<AtencionClinica> HistorialAtenciones
    {
        get => _historialAtenciones;
        set => SetProperty(ref _historialAtenciones, value);
    }

    public ObservableCollection<Paciente> PacientesDisponibles
    {
        get => _pacientesDisponibles;
        set => SetProperty(ref _pacientesDisponibles, value);
    }

    public string CriterioBusquedaPaciente
    {
        get => _criterioBusquedaPaciente;
        set
        {
            if (SetProperty(ref _criterioBusquedaPaciente, value))
            {
                _ = BuscarPacientesAsync();
            }
        }
    }

    public ICommand NuevaConsultaCommand { get; }
    public ICommand IrAVacunacionCommand { get; }
    public ICommand VolverCommand { get; }
    public ICommand BuscarPacienteCommand { get; }

    public async Task InicializarAsync()
    {
        await BuscarPacientesAsync();
        if (PacienteActual == null && PacientesDisponibles.Any())
        {
            PacienteActual = PacientesDisponibles.FirstOrDefault();
        }
    }

    public async Task BuscarPacientesAsync()
    {
        try
        {
            var lista = await _clinicaService.BuscarPacientesAsync(CriterioBusquedaPaciente);
            PacientesDisponibles = new ObservableCollection<Paciente>(lista);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error", $"Error consultando pacientes: {ex.Message}");
        }
    }

    public async Task CargarHistorialAsync(int pacienteId)
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            // Cargar detalle completo del paciente con relaciones
            var detalle = await _clinicaService.ObtenerPacientePorIdAsync(pacienteId);
            if (detalle != null)
            {
                _pacienteActual = detalle;
                OnPropertyChanged(nameof(PacienteActual));
            }

            var atenciones = await _clinicaService.ObtenerHistorialPacienteAsync(pacienteId);
            HistorialAtenciones = new ObservableCollection<AtencionClinica>(atenciones);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error de lectura", $"No fue posible cargar el historial clínico: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AbrirNuevaConsulta()
    {
        if (PacienteActual == null)
        {
            _dialogService.ShowError("Atención", "Debe seleccionar un paciente antes de abrir una nueva consulta.");
            return;
        }

        _dialogService.ShowNuevaAtencionModal(PacienteActual.Id, async atencion =>
        {
            _dialogService.ShowInformation("Atención Guardada", "La atención médica ha sido registrada de forma inmutable.");
            await CargarHistorialAsync(PacienteActual.Id);
        });
    }

    private void NavegarAVacunacion()
    {
        _navigationService.NavigateTo<InmunizacionesViewModel>();
    }

    private void VolverAPacientes()
    {
        _navigationService.NavigateTo<PacientesViewModel>();
    }
}
