using System.Windows.Controls;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class PacientesView : UserControl
{
    public PacientesView()
    {
        InitializeComponent();
    }

    public PacientesView(PacientesViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
