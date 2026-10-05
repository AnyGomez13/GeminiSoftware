using System.Windows.Controls;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class InmunizacionesView : UserControl
{
    public InmunizacionesView()
    {
        InitializeComponent();
    }

    public InmunizacionesView(InmunizacionesViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
