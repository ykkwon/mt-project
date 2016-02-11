using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;

namespace dbApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        //open.Filter = "Video File (*.mp4)";
            
        public MainWindow()
        {
            InitializeComponent();
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Video File (*.mp4)|*.wav;";

            if (open.ShowDialog() != true) return;
            FingerprintManager fp = new FingerprintManager(open.FileName);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}