using Avalonia.Controls;
using N6700BControllerApp.ViewModels;

namespace N6700BControllerApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}