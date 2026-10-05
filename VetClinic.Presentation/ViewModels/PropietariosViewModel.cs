using System.Collections.ObjectModel;
using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Presentation.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class PropietariosViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    private string _criterioBusqueda = string.Empty;
    private Propietario? _propietarioSeleccionado;
    private ObservableCollection<Propietario> _propietarios = new();
    private ObservableCollection<Paciente> _pacientesDelPropietario = new();

    public PropietariosViewModel(
        IClinicaService clinicaService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clinicaService = clinicaService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        BuscarCommand = new AsyncRelayCommand(EjecutarBusquedaAsync);
        AbrirCreacionPropietarioCommand = new RelayCommand(AbrirCreacionPropietario);
        AbrirEdicionPropietarioCommand = new RelayCommand<Propietario>(AbrirEdicionPropietario);
        VerHistoriaClinicaCommand = new RelayCommand<Paciente>(VerHistoriaClinica);
        NuevoPacienteCommand = new RelayCommand<Propietario>(NuevoPacienteParaPropietario);

        // Carga inicial no bloqueante
        _ = EjecutarBusquedaAsync();
    }

    public string CriterioBusqueda
    {
        get => _criterioBusqueda;
        set
        {
            if (SetProperty(ref _criterioBusqueda, value))
            {
                // Búsqueda en tiempo real conforme el usuario tipea
                _ = EjecutarBusquedaAsync();
            }
        }
    }

    public ObservableCollection<Propietario> Propietarios
    {
        get => _propietarios;
        set => SetProperty(ref _propietarios, value);
    }

    public Propietario? PropietarioSeleccionado
    {
        get => _propietarioSeleccionado;
        set
        {
            if (SetProperty(ref _propietarioSeleccionado, value))
            {
                CargarPacientesDePropietario(value);
            }
        }
    }

    public ObservableCollection<Paciente> PacientesDelPropietario
    {
        get => _pacientesDelPropietario;
        set => SetProperty(ref _pacientesDelPropietario, value);
    }

    public ICommand BuscarCommand { get; }
    public ICommand AbrirCreacionPropietarioCommand { get; }
    public ICommand AbrirEdicionPropietarioCommand { get; }
    public ICommand VerHistoriaClinicaCommand { get; }
    public ICommand NuevoPacienteCommand { get; }

    public async Task EjecutarBusquedaAsync()
    {
        try
        {
            var resultados = await _clinicaService.BuscarPropietariosAsync(CriterioBusqueda);
            Propietarios = new ObservableCollection<Propietario>(resultados);

            if (PropietarioSeleccionado != null)
            {
                var coincidencia = Propietarios.FirstOrDefault(p => p.Id == PropietarioSeleccionado.Id);
                PropietarioSeleccionado = coincidencia ?? Propietarios.FirstOrDefault();
            }
            else
            {
                PropietarioSeleccionado = Propietarios.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error de búsqueda", $"No fue posible consultar propietarios: {ex.Message}");
        }
    }

    private void CargarPacientesDePropietario(Propietario? propietario)
    {
        if (propietario == null)
        {
            PacientesDelPropietario = new ObservableCollection<Paciente>();
            return;
        }

        PacientesDelPropietario = new ObservableCollection<Paciente>(propietario.Pacientes.Where(p => !p.IsDeleted));
    }

    private void AbrirCreacionPropietario()
    {
        _dialogService.ShowPropietarioModal(null, nuevo =>
        {
            _ = EjecutarBusquedaAsync();
            _dialogService.ShowInformation("Éxito", $"Propietario {nuevo.GetNombreCompleto()} registrado correctamente.");
        });
    }

    private void AbrirEdicionPropietario(Propietario? propietario)
    {
        var target = propietario ?? PropietarioSeleccionado;
        if (target == null) return;

        _dialogService.ShowPropietarioModal(target, actualizado =>
        {
            _ = EjecutarBusquedaAsync();
            _dialogService.ShowInformation("Éxito", $"Propietario {actualizado.GetNombreCompleto()} actualizado correctamente.");
        });
    }

    private void VerHistoriaClinica(Paciente? paciente)
    {
        if (paciente == null) return;
        _navigationService.NavigateTo<HistoriaClinicaViewModel>();
    }

    private void NuevoPacienteParaPropietario(Propietario? propietario)
    {
        _navigationService.NavigateTo<PacientesViewModel>();
    }
}
