using System.Windows;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class NuevaAtencionModalView : Window
{
    public NuevaAtencionModalView()
    {
        InitializeComponent();
    }

    public NuevaAtencionModalView(NuevaAtencionViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.IsGuardado;
            Close();
        };
    }
}
