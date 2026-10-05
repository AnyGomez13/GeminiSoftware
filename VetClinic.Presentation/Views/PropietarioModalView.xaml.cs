using System.Windows;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class PropietarioModalView : Window
{
    public PropietarioModalView()
    {
        InitializeComponent();
    }

    public PropietarioModalView(PropietarioModalViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.IsGuardado;
            Close();
        };
    }
}
