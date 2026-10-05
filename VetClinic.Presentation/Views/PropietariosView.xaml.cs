using System.Windows.Controls;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class PropietariosView : UserControl
{
    public PropietariosView()
    {
        InitializeComponent();
    }

    public PropietariosView(PropietariosViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
