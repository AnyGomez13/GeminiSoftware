using System.Collections.ObjectModel;
using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Domain.ValueObjects;
using VetClinic.Presentation.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class PacientesViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    // Campos de Búsqueda y Censo
    private string _criterioBusqueda = string.Empty;
    private ObservableCollection<Paciente> _pacientes = new();
    private Paciente? _pacienteSeleccionado;

    // Campos del Formulario de Paciente
    private int _id;
    private Propietario? _propietarioSeleccionado;
    private ObservableCollection<Propietario> _propietariosDisponibles = new();
    private string _nombre = string.Empty;
    private Especie _especie = Especie.Canino;
    private string _raza = string.Empty;
    private Sexo _sexo = Sexo.Macho;
    private DateTime _fechaNacimiento = DateTime.Today;
    private bool _esFechaEstimada;
    private double _pesoActualKg = 5.0;
    private string? _colorSenas;
    private EstadoReproductivo _estadoReproductivo = EstadoReproductivo.Entero;
    private string _edadCalculada = "0 días";
    private string _formularioErrorMessage = string.Empty;
    private bool _hasFormularioError;

    public PacientesViewModel(
        IClinicaService clinicaService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clinicaService = clinicaService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        BuscarCommand = new AsyncRelayCommand(EjecutarBusquedaAsync);
        GuardarPacienteCommand = new AsyncRelayCommand(GuardarPacienteAsync);
        NuevoFormularioCommand = new RelayCommand(LimpiarFormulario);
        CargarParaEdicionCommand = new RelayCommand<Paciente>(CargarParaEdicion);
        CrearPropietarioEnCalienteCommand = new RelayCommand(CrearPropietarioEnCaliente);
        IrAHistoriaClinicaCommand = new RelayCommand<Paciente>(IrAHistoriaClinica);

        ActualizarEdadCalculada();
        _ = CargarDatosInicialesAsync();
    }

    public string CriterioBusqueda
    {
        get => _criterioBusqueda;
        set
        {
            if (SetProperty(ref _criterioBusqueda, value))
            {
                _ = EjecutarBusquedaAsync();
            }
        }
    }

    public ObservableCollection<Paciente> Pacientes
    {
        get => _pacientes;
        set => SetProperty(ref _pacientes, value);
    }

    public Paciente? PacienteSeleccionado
    {
        get => _pacienteSeleccionado;
        set
        {
            if (SetProperty(ref _pacienteSeleccionado, value) && value != null)
            {
                CargarParaEdicion(value);
            }
        }
    }

    public ObservableCollection<Propietario> PropietariosDisponibles
    {
        get => _propietariosDisponibles;
        set => SetProperty(ref _propietariosDisponibles, value);
    }

    public Propietario? PropietarioSeleccionado
    {
        get => _propietarioSeleccionado;
        set => SetProperty(ref _propietarioSeleccionado, value);
    }

    public int Id
    {
        get => _id;
        set
        {
            if (SetProperty(ref _id, value))
            {
                OnPropertyChanged(nameof(EsEdicion));
                OnPropertyChanged(nameof(TituloFormulario));
            }
        }
    }

    public bool EsEdicion => Id > 0;
    public string TituloFormulario => EsEdicion ? "Modificar Ficha de Paciente" : "Registrar Nuevo Paciente";

    public string Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    public Especie Especie
    {
        get => _especie;
        set => SetProperty(ref _especie, value);
    }

    public string Raza
    {
        get => _raza;
        set => SetProperty(ref _raza, value);
    }

    public Sexo Sexo
    {
        get => _sexo;
        set => SetProperty(ref _sexo, value);
    }

    public DateTime FechaNacimiento
    {
        get => _fechaNacimiento;
        set
        {
            if (SetProperty(ref _fechaNacimiento, value))
            {
                ActualizarEdadCalculada();
            }
        }
    }

    public bool EsFechaEstimada
    {
        get => _esFechaEstimada;
        set => SetProperty(ref _esFechaEstimada, value);
    }

    public double PesoActualKg
    {
        get => _pesoActualKg;
        set => SetProperty(ref _pesoActualKg, value);
    }

    public string? ColorSenas
    {
        get => _colorSenas;
        set => SetProperty(ref _colorSenas, value);
    }

    public EstadoReproductivo EstadoReproductivo
    {
        get => _estadoReproductivo;
        set => SetProperty(ref _estadoReproductivo, value);
    }

    public string EdadCalculada
    {
        get => _edadCalculada;
        private set => SetProperty(ref _edadCalculada, value);
    }

    public string FormularioErrorMessage
    {
        get => _formularioErrorMessage;
        set => SetProperty(ref _formularioErrorMessage, value);
    }

    public bool HasFormularioError
    {
        get => _hasFormularioError;
        set => SetProperty(ref _hasFormularioError, value);
    }

    public ICommand BuscarCommand { get; }
    public ICommand GuardarPacienteCommand { get; }
    public ICommand NuevoFormularioCommand { get; }
    public ICommand CargarParaEdicionCommand { get; }
    public ICommand CrearPropietarioEnCalienteCommand { get; }
    public ICommand IrAHistoriaClinicaCommand { get; }

    public async Task CargarDatosInicialesAsync()
    {
        await CargarPropietariosDisponiblesAsync();
        await EjecutarBusquedaAsync();
    }

    public async Task CargarPropietariosDisponiblesAsync()
    {
        var propietarios = await _clinicaService.BuscarPropietariosAsync(string.Empty);
        PropietariosDisponibles = new ObservableCollection<Propietario>(propietarios);
    }

    public async Task EjecutarBusquedaAsync()
    {
        try
        {
            var resultados = await _clinicaService.BuscarPacientesAsync(CriterioBusqueda);
            Pacientes = new ObservableCollection<Paciente>(resultados);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error de búsqueda", $"No fue posible consultar pacientes: {ex.Message}");
        }
    }

    public void ActualizarEdadCalculada()
    {
        var dummy = new Paciente { FechaNacimiento = FechaNacimiento };
        EdadCalculada = dummy.EdadFormateada;
    }

    public void LimpiarFormulario()
    {
        Id = 0;
        Nombre = string.Empty;
        Especie = Especie.Canino;
        Raza = string.Empty;
        Sexo = Sexo.Macho;
        FechaNacimiento = DateTime.Today;
        EsFechaEstimada = false;
        PesoActualKg = 5.0;
        ColorSenas = null;
        EstadoReproductivo = EstadoReproductivo.Entero;
        PropietarioSeleccionado = PropietariosDisponibles.FirstOrDefault();
        HasFormularioError = false;
        FormularioErrorMessage = string.Empty;
        ActualizarEdadCalculada();
    }

    public void CargarParaEdicion(Paciente? paciente)
    {
        if (paciente == null) return;

        Id = paciente.Id;
        Nombre = paciente.Nombre;
        Especie = paciente.Especie;
        Raza = paciente.Raza;
        Sexo = paciente.Sexo;
        FechaNacimiento = paciente.FechaNacimiento;
        EsFechaEstimada = paciente.EsFechaEstimada;
        PesoActualKg = paciente.PesoActualKg;
        ColorSenas = paciente.ColorSenas;
        EstadoReproductivo = paciente.EstadoReproductivo;
        PropietarioSeleccionado = PropietariosDisponibles.FirstOrDefault(p => p.Id == paciente.PropietarioId);

        HasFormularioError = false;
        FormularioErrorMessage = string.Empty;
        ActualizarEdadCalculada();
    }

    public async Task GuardarPacienteAsync()
    {
        if (IsBusy) return;

        if (PropietarioSeleccionado == null)
        {
            MostrarError("Debe asociar obligatoriamente un acudiente/propietario.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MostrarError("El nombre del paciente es obligatorio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Raza))
        {
            MostrarError("La raza del paciente es obligatoria.");
            return;
        }

        if (FechaNacimiento.Date > DateTime.Today)
        {
            MostrarError("La fecha de nacimiento no puede ser posterior a la fecha actual.");
            return;
        }

        var pesoValidation = PesoCorporal.Crear(PesoActualKg);
        if (!pesoValidation.IsSuccess)
        {
            MostrarError(pesoValidation.Error.Message);
            return;
        }

        IsBusy = true;
        HasFormularioError = false;

        try
        {
            if (Id == 0)
            {
                var nuevo = new Paciente
                {
                    PropietarioId = PropietarioSeleccionado.Id,
                    Nombre = Nombre.Trim(),
                    Especie = Especie,
                    Raza = Raza.Trim(),
                    Sexo = Sexo,
                    FechaNacimiento = FechaNacimiento.Date,
                    EsFechaEstimada = EsFechaEstimada,
                    PesoActualKg = pesoValidation.Value.Valor,
                    ColorSenas = ColorSenas?.Trim(),
                    EstadoReproductivo = EstadoReproductivo
                };

                var resultado = await _clinicaService.RegistrarPacienteAsync(nuevo);
                if (resultado.IsSuccess)
                {
                    _dialogService.ShowInformation("Éxito", $"Paciente {nuevo.Nombre} registrado con éxito.");
                    LimpiarFormulario();
                    await EjecutarBusquedaAsync();
                }
                else
                {
                    MostrarError(resultado.Error.Message);
                }
            }
            else
            {
                var pacienteExistente = await _clinicaService.ObtenerPacientePorIdAsync(Id);
                if (pacienteExistente == null)
                {
                    MostrarError("El paciente no existe en el sistema.");
                    return;
                }

                pacienteExistente.PropietarioId = PropietarioSeleccionado.Id;
                pacienteExistente.Nombre = Nombre.Trim();
                pacienteExistente.Especie = Especie;
                pacienteExistente.Raza = Raza.Trim();
                pacienteExistente.Sexo = Sexo;
                pacienteExistente.FechaNacimiento = FechaNacimiento.Date;
                pacienteExistente.EsFechaEstimada = EsFechaEstimada;
                pacienteExistente.PesoActualKg = pesoValidation.Value.Valor;
                pacienteExistente.ColorSenas = ColorSenas?.Trim();
                pacienteExistente.EstadoReproductivo = EstadoReproductivo;

                var resultado = await _clinicaService.ActualizarPacienteAsync(pacienteExistente);
                if (resultado.IsSuccess)
                {
                    _dialogService.ShowInformation("Éxito", $"Ficha de {pacienteExistente.Nombre} actualizada con éxito.");
                    await EjecutarBusquedaAsync();
                }
                else
                {
                    MostrarError(resultado.Error.Message);
                }
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error al guardar paciente: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CrearPropietarioEnCaliente()
    {
        _dialogService.ShowPropietarioModal(null, async nuevoPropietario =>
        {
            await CargarPropietariosDisponiblesAsync();
            PropietarioSeleccionado = PropietariosDisponibles.FirstOrDefault(p => p.Id == nuevoPropietario.Id);
            _dialogService.ShowInformation("Propietario Vinculado", $"Se ha asociado a {nuevoPropietario.GetNombreCompleto()} al formulario actual.");
        });
    }

    private void IrAHistoriaClinica(Paciente? paciente)
    {
        var target = paciente ?? PacienteSeleccionado;
        if (target == null) return;
        _navigationService.NavigateTo<HistoriaClinicaViewModel>();
    }

    private void MostrarError(string mensaje)
    {
        FormularioErrorMessage = mensaje;
        HasFormularioError = true;
    }
}
