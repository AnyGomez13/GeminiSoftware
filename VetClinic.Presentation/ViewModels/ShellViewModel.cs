using System.Windows.Input;
using VetClinic.Presentation.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private string _usuarioActivo = "Dr. Fabio / Dr. William";
    private string _fechaHoraActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        NavigateCommand = new RelayCommand<string>(EjecutarNavegacion);
        CerrarSesionCommand = new RelayCommand(EjecutarCerrarSesion);
    }

    public ViewModelBase? CurrentViewModel => _navigationService.CurrentViewModel;

    public string UsuarioActivo
    {
        get => _usuarioActivo;
        set => SetProperty(ref _usuarioActivo, value);
    }

    public string FechaHoraActual
    {
        get => _fechaHoraActual;
        set => SetProperty(ref _fechaHoraActual, value);
    }

    public ICommand NavigateCommand { get; }
    public ICommand CerrarSesionCommand { get; }

    public event Action? CerrarSesionSolicitado;

    public void EjecutarNavegacion(string? destino)
    {
        if (string.IsNullOrWhiteSpace(destino)) return;

        switch (destino.Trim().ToLowerInvariant())
        {
            case "propietarios":
                _navigationService.NavigateTo<PropietariosViewModel>();
                StatusMessage = "Módulo de Directorio de Propietarios";
                break;
            case "pacientes":
                _navigationService.NavigateTo<PacientesViewModel>();
                StatusMessage = "Módulo de Censo de Pacientes";
                break;
            case "historiaclinica":
                _navigationService.NavigateTo<HistoriaClinicaViewModel>();
                StatusMessage = "Módulo de Historia Clínica Unificada";
                break;
            case "inmunizaciones":
                _navigationService.NavigateTo<InmunizacionesViewModel>();
                StatusMessage = "Módulo de Vacunación y Carnet Digital";
                break;
            case "recordatorios":
                _navigationService.NavigateTo<RecordatoriosViewModel>();
                StatusMessage = "Módulo de Recordatorios y Notificaciones Gratuitas";
                break;
            default:
                StatusMessage = $"Módulo: {destino}";
                break;
        }
    }

    private void EjecutarCerrarSesion()
    {
        CerrarSesionSolicitado?.Invoke();
    }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }
}
