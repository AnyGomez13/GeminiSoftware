using System.Windows.Controls;
using VetClinic.Presentation.ViewModels;

namespace VetClinic.Presentation.Views;

public partial class RecordatoriosView : UserControl
{
    public RecordatoriosView()
    {
        InitializeComponent();
    }

    public RecordatoriosView(RecordatoriosViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
