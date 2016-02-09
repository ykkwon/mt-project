

using Microsoft.Win32;
using System.Windows;

namespace dbApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog open = new OpenFileDialog();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (open.ShowDialog() != true) return;

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
