using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Presentation.ViewModels;
using VetClinic.Presentation.Views;

namespace VetClinic.Presentation.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ShowInformation(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool ShowConfirmation(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public string? ShowSaveFileDialog(string title, string defaultFileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            FileName = defaultFileName,
            Filter = filter
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool? ShowPropietarioModal(Propietario? propietario = null, Action<Propietario>? onSaved = null)
    {
        var clinicaService = _serviceProvider.GetRequiredService<IClinicaService>();
        var viewModel = new PropietarioModalViewModel(clinicaService);

        if (propietario != null)
        {
            viewModel.CargarParaEdicion(propietario);
        }

        var window = new PropietarioModalView(viewModel);
        var result = window.ShowDialog();

        if (result == true && viewModel.PropietarioResult != null)
        {
            onSaved?.Invoke(viewModel.PropietarioResult);
        }

        return result;
    }

    public bool? ShowNuevaAtencionModal(int pacienteId, Action<AtencionClinica>? onSaved = null)
    {
        var clinicaService = _serviceProvider.GetRequiredService<IClinicaService>();
        var viewModel = new NuevaAtencionViewModel(clinicaService);

        // Cargar paciente si existe
        var task = clinicaService.ObtenerPacientePorIdAsync(pacienteId);
        task.Wait();
        var paciente = task.Result;
        if (paciente != null)
        {
            viewModel.ConfigurarPaciente(paciente);
        }
        else
        {
            viewModel.PacienteId = pacienteId;
        }

        var window = new NuevaAtencionModalView(viewModel);
        var result = window.ShowDialog();

        if (result == true && viewModel.AtencionResult != null)
        {
            onSaved?.Invoke(viewModel.AtencionResult);
        }

        return result;
    }
}
