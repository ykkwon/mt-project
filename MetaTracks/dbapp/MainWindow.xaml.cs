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
        private OpenFileDialog open;
            public MainWindow()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            open = new OpenFileDialog();
            open.Filter = "Video File (*.mp4)|*.wav;";

                if (open.ShowDialog() != true) return;
                fp = new FingerprintManager(open.FileName);
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.SplitWavFile(open.FileName, 1000);
        }

        public void WriteToForeground(string output)
        {
            fg_label.Content = output;
        }
    }
}