using System.Windows;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class ShellView : Window
{
    public ShellView(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
