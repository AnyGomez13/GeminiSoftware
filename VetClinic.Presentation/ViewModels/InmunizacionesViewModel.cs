using System.Collections.ObjectModel;
using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Presentation.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class InmunizacionesViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;
    private readonly IPdfExportService _pdfExportService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    private Paciente? _pacienteActual;
    private ObservableCollection<Inmunizacion> _inmunizaciones = new();
    private ObservableCollection<Paciente> _pacientesDisponibles = new();
    private ObservableCollection<Veterinario> _veterinarios = new();

    // Formulario de registro de biológico (RF-08, RN-08)
    private TipoBiologico _tipoBiologico = TipoBiologico.Vacuna;
    private string _nombreProducto = string.Empty;
    private string? _loteFabricante;
    private DateTime _fechaAplicacion = DateTime.Today;
    private DateTime _fechaRefuerzo = DateTime.Today.AddYears(1);
    private string? _observaciones;
    private Veterinario? _veterinarioSeleccionado;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    public InmunizacionesViewModel(
        IClinicaService clinicaService,
        IPdfExportService pdfExportService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clinicaService = clinicaService;
        _pdfExportService = pdfExportService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        RegistrarDosisCommand = new AsyncRelayCommand(RegistrarDosisAsync);
        ExportarCarnetPdfCommand = new AsyncRelayCommand(ExportarCarnetPdfAsync);
        IrARecordatoriosCommand = new RelayCommand(NavegarARecordatorios);

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
                    _ = CargarInmunizacionesAsync(value.Id);
                }
                else
                {
                    Inmunizaciones = new ObservableCollection<Inmunizacion>();
                }
            }
        }
    }

    public bool TienePacienteSeleccionado => PacienteActual != null;

    public ObservableCollection<Inmunizacion> Inmunizaciones
    {
        get => _inmunizaciones;
        set => SetProperty(ref _inmunizaciones, value);
    }

    public ObservableCollection<Paciente> PacientesDisponibles
    {
        get => _pacientesDisponibles;
        set => SetProperty(ref _pacientesDisponibles, value);
    }

    public ObservableCollection<Veterinario> Veterinarios
    {
        get => _veterinarios;
        set => SetProperty(ref _veterinarios, value);
    }

    public TipoBiologico TipoBiologico
    {
        get => _tipoBiologico;
        set => SetProperty(ref _tipoBiologico, value);
    }

    public string NombreProducto
    {
        get => _nombreProducto;
        set => SetProperty(ref _nombreProducto, value);
    }

    public string? LoteFabricante
    {
        get => _loteFabricante;
        set => SetProperty(ref _loteFabricante, value);
    }

    public DateTime FechaAplicacion
    {
        get => _fechaAplicacion;
        set => SetProperty(ref _fechaAplicacion, value);
    }

    public DateTime FechaRefuerzo
    {
        get => _fechaRefuerzo;
        set => SetProperty(ref _fechaRefuerzo, value);
    }

    public string? Observaciones
    {
        get => _observaciones;
        set => SetProperty(ref _observaciones, value);
    }

    public Veterinario? VeterinarioSeleccionado
    {
        get => _veterinarioSeleccionado;
        set => SetProperty(ref _veterinarioSeleccionado, value);
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

    public ICommand RegistrarDosisCommand { get; }
    public ICommand ExportarCarnetPdfCommand { get; }
    public ICommand IrARecordatoriosCommand { get; }

    public async Task InicializarAsync()
    {
        try
        {
            var vets = await _clinicaService.ObtenerVeterinariosActivosAsync();
            Veterinarios = new ObservableCollection<Veterinario>(vets);
            VeterinarioSeleccionado = Veterinarios.FirstOrDefault();

            var pacientes = await _clinicaService.BuscarPacientesAsync(string.Empty);
            PacientesDisponibles = new ObservableCollection<Paciente>(pacientes);
            if (PacienteActual == null && PacientesDisponibles.Any())
            {
                PacienteActual = PacientesDisponibles.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error cargando datos de vacunación: {ex.Message}");
        }
    }

    public async Task CargarInmunizacionesAsync(int pacienteId)
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var detalle = await _clinicaService.ObtenerPacientePorIdAsync(pacienteId);
            if (detalle != null)
            {
                _pacienteActual = detalle;
                OnPropertyChanged(nameof(PacienteActual));
            }

            var lista = await _clinicaService.ObtenerInmunizacionesPacienteAsync(pacienteId);
            Inmunizaciones = new ObservableCollection<Inmunizacion>(lista);
        }
        catch (Exception ex)
        {
            MostrarError($"Error cargando historial de vacunas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task RegistrarDosisAsync()
    {
        if (IsBusy) return;

        if (PacienteActual == null)
        {
            MostrarError("Debe seleccionar un paciente para registrar la dosis.");
            return;
        }

        if (VeterinarioSeleccionado == null || VeterinarioSeleccionado.Id <= 0)
        {
            MostrarError("Debe seleccionar al Veterinario responsable (Dr. Fabio o Dr. William).");
            return;
        }

        if (string.IsNullOrWhiteSpace(NombreProducto))
        {
            MostrarError("El nombre del biológico/vacuna es obligatorio.");
            return;
        }

        // Validación estricta RN-08: Refuerzo posterior a la aplicación
        if (FechaRefuerzo.Date <= FechaAplicacion.Date)
        {
            MostrarError("La fecha de próximo refuerzo debe ser estrictamente posterior a la fecha de aplicación.");
            return;
        }

        IsBusy = true;
        LimpiarError();

        try
        {
            var inmunizacion = new Inmunizacion
            {
                PacienteId = PacienteActual.Id,
                VeterinarioId = VeterinarioSeleccionado.Id,
                TipoBiologico = TipoBiologico,
                NombreProducto = NombreProducto.Trim(),
                LoteFabricante = LoteFabricante?.Trim(),
                FechaAplicacion = FechaAplicacion.Date,
                FechaRefuerzo = FechaRefuerzo.Date,
                Observaciones = Observaciones?.Trim()
            };

            var resultado = await _clinicaService.RegistrarInmunizacionAsync(inmunizacion);
            if (resultado.IsSuccess)
            {
                _dialogService.ShowInformation("Éxito", $"Dosis de {inmunizacion.NombreProducto} registrada correctamente.");
                NombreProducto = string.Empty;
                LoteFabricante = null;
                Observaciones = null;
                FechaAplicacion = DateTime.Today;
                FechaRefuerzo = DateTime.Today.AddYears(1);
                await CargarInmunizacionesAsync(PacienteActual.Id);
            }
            else
            {
                MostrarError(resultado.Error.Message);
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error al registrar dosis: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ExportarCarnetPdfAsync()
    {
        if (PacienteActual == null)
        {
            _dialogService.ShowError("Atención", "Debe seleccionar un paciente antes de exportar el carnet.");
            return;
        }

        var nombreArchivo = $"Carnet_{PacienteActual.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";
        var rutaGuardado = _dialogService.ShowSaveFileDialog("Exportar Carnet Digital de Vacunación", nombreArchivo, "Archivo PDF (*.pdf)|*.pdf");
        if (string.IsNullOrWhiteSpace(rutaGuardado)) return;

        IsBusy = true;
        try
        {
            var cronometro = System.Diagnostics.Stopwatch.StartNew();
            var resultado = await _pdfExportService.GenerarCarnetVacunacionAsync(PacienteActual.Id, rutaGuardado);
            cronometro.Stop();

            if (resultado.IsSuccess)
            {
                _dialogService.ShowInformation(
                    "Carnet Generado con Éxito",
                    $"El Carnet Digital en PDF ha sido guardado exitosamente en:\n{resultado.Value}\n\nTiempo de procesamiento: {cronometro.ElapsedMilliseconds} ms (RNF-06: < 3000 ms).");
            }
            else
            {
                _dialogService.ShowError("Error de Exportación", resultado.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error de Exportación", $"No se pudo generar el carnet PDF: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NavegarARecordatorios()
    {
        _navigationService.NavigateTo<RecordatoriosViewModel>();
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
