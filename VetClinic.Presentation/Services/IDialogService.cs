using VetClinic.Domain.Entities;

namespace VetClinic.Presentation.Services;

public interface IDialogService
{
    void ShowInformation(string title, string message);
    void ShowError(string title, string message);
    bool ShowConfirmation(string title, string message);
    string? ShowSaveFileDialog(string title, string defaultFileName, string filter);
    bool? ShowPropietarioModal(Propietario? propietario = null, Action<Propietario>? onSaved = null);
    bool? ShowNuevaAtencionModal(int pacienteId, Action<AtencionClinica>? onSaved = null);
}
