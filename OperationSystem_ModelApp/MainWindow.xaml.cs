using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using OperationSystem_ModelApp.ViewModel;

namespace OperationSystem_ModelApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void FromFile_Click(object sender, RoutedEventArgs e)
        {
            var OpenDialog = new OpenFileDialog();
            var vm = (MainViewModel)DataContext;

            if (OpenDialog.ShowDialog() == true)
            {
                vm.PathToFile = OpenDialog.FileName;
            }
        }
    }
}