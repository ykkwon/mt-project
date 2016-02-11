using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using dbApp.Fingerprint;
using NAudio.Dmo;

namespace dbApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FingerprintManager fp;
        
        //open.Filter = "Video File (*.mp4)";
            
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Video File (*.mp4)|*.mp4;";

                if (open.ShowDialog() != true) return;
                fp = new FingerprintManager(open.FileName);
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            fp.ReceiveMovie();
        }

        public void UpdateOutput(string print)
        {
            output.Content = print;
        }
    }
}