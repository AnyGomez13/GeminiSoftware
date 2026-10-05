using System.Windows;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
