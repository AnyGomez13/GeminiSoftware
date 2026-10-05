using System.Collections.ObjectModel;
using System.Windows.Input;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Presentation.Services;
using VetClinic.Presentation.ViewModels.Common;

namespace VetClinic.Presentation.ViewModels;

public class RecordatoriosViewModel : ViewModelBase
{
    private readonly IClinicaService _clinicaService;
    private readonly IExternalLauncherService _launcherService;
    private readonly IDialogService _dialogService;

    private int _diasAnticipacion = 7;
    private ObservableCollection<Inmunizacion> _proximosRefuerzos = new();
    private Inmunizacion? _inmunizacionSeleccionada;
    private string _mensajePrevisualizado = string.Empty;

    public RecordatoriosViewModel(
        IClinicaService clinicaService,
        IExternalLauncherService launcherService,
        IDialogService dialogService)
    {
        _clinicaService = clinicaService;
        _launcherService = launcherService;
        _dialogService = dialogService;

        CargarRefuerzosCommand = new AsyncRelayCommand(CargarRefuerzosAsync);
        EnviarWhatsAppCommand = new RelayCommand<Inmunizacion>(EnviarWhatsApp);
        EnviarCorreoCommand = new RelayCommand<Inmunizacion>(EnviarCorreo);

        _ = CargarRefuerzosAsync();
    }

    public int DiasAnticipacion
    {
        get => _diasAnticipacion;
        set
        {
            if (SetProperty(ref _diasAnticipacion, value))
            {
                _ = CargarRefuerzosAsync();
            }
        }
    }

    public ObservableCollection<Inmunizacion> ProximosRefuerzos
    {
        get => _proximosRefuerzos;
        set => SetProperty(ref _proximosRefuerzos, value);
    }

    public Inmunizacion? InmunizacionSeleccionada
    {
        get => _inmunizacionSeleccionada;
        set
        {
            if (SetProperty(ref _inmunizacionSeleccionada, value))
            {
                ActualizarMensajePrevisualizado(value);
            }
        }
    }

    public string MensajePrevisualizado
    {
        get => _mensajePrevisualizado;
        set => SetProperty(ref _mensajePrevisualizado, value);
    }

    public ICommand CargarRefuerzosCommand { get; }
    public ICommand EnviarWhatsAppCommand { get; }
    public ICommand EnviarCorreoCommand { get; }

    public async Task CargarRefuerzosAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var refuerzos = await _clinicaService.ObtenerProximosRefuerzosAsync(DiasAnticipacion);
            ProximosRefuerzos = new ObservableCollection<Inmunizacion>(refuerzos);
            InmunizacionSeleccionada = ProximosRefuerzos.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error", $"No fue posible cargar la lista de refuerzos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ActualizarMensajePrevisualizado(Inmunizacion? inmunizacion)
    {
        if (inmunizacion == null || inmunizacion.Paciente == null)
        {
            MensajePrevisualizado = "Seleccione un paciente de la lista para previsualizar el aviso de recordatorio.";
            return;
        }

        var prop = inmunizacion.Paciente.Propietario;
        var nombreAcudiente = prop != null ? prop.GetNombreCompleto() : "Estimado(a) Propietario(a)";

        MensajePrevisualizado =
            $"Hola {nombreAcudiente}, cordial saludo de parte de VetClinic Pro.\n\n" +
            $"Le recordamos que su mascota *{inmunizacion.Paciente.Nombre}* tiene programada la aplicación de su refuerzo de *{inmunizacion.NombreProducto}* para el día *{inmunizacion.FechaRefuerzo:dd/MM/yyyy}*.\n\n" +
            $"Por favor acérquese a nuestra clínica o comuníquese con nosotros para confirmar su cita y mantener la protección biológica al día.";
    }

    public void EnviarWhatsApp(Inmunizacion? inmunizacion)
    {
        var target = inmunizacion ?? InmunizacionSeleccionada;
        if (target?.Paciente?.Propietario == null)
        {
            _dialogService.ShowError("Error", "El registro seleccionado no tiene un acudiente con datos de contacto válidos.");
            return;
        }

        var telefono = target.Paciente.Propietario.Telefono;
        if (string.IsNullOrWhiteSpace(telefono))
        {
            _dialogService.ShowError("Sin Teléfono", "El acudiente no tiene registrado un número telefónico.");
            return;
        }

        var mensaje =
            $"Hola {target.Paciente.Propietario.GetNombreCompleto()}, le recordamos que {target.Paciente.Nombre} tiene programado su refuerzo de {target.NombreProducto} para el {target.FechaRefuerzo:dd/MM/yyyy}. Lo esperamos en VetClinic Pro.";

        var resultado = _launcherService.AbrirWhatsApp(telefono, mensaje);
        if (resultado.IsSuccess)
        {
            _dialogService.ShowInformation("WhatsApp Abierto", $"Se ha abierto el enlace wa.me para {target.Paciente.Propietario.GetNombreCompleto()}. Cero costo de mensajería.");
        }
        else
        {
            _dialogService.ShowError("Error de Apertura", resultado.Error.Message);
        }
    }

    public void EnviarCorreo(Inmunizacion? inmunizacion)
    {
        var target = inmunizacion ?? InmunizacionSeleccionada;
        if (target?.Paciente?.Propietario == null)
        {
            _dialogService.ShowError("Error", "El registro no tiene datos de propietario vinculados.");
            return;
        }

        var email = target.Paciente.Propietario.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            _dialogService.ShowError("Sin Correo", "El acudiente no tiene registrado correo electrónico.");
            return;
        }

        var asunto = $"Recordatorio de Vacunación - {target.Paciente.Nombre} (VetClinic Pro)";
        var resultado = _launcherService.AbrirCorreo(email, asunto, MensajePrevisualizado);
        if (resultado.IsSuccess)
        {
            _dialogService.ShowInformation("Correo Abierto", $"Se ha abierto el cliente de correo predeterminado para {email}.");
        }
        else
        {
            _dialogService.ShowError("Error de Apertura", resultado.Error.Message);
        }
    }
}
