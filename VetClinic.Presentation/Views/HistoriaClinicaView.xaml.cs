using System.Windows.Controls;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class HistoriaClinicaView : UserControl
{
    public HistoriaClinicaView()
    {
        InitializeComponent();
    }

    public HistoriaClinicaView(HistoriaClinicaViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
