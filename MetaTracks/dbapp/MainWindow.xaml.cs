using Microsoft.Win32;
using System;
using System.Windows;
using dbApp.Fingerprint;
using System.Threading;

namespace dbApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Video returnedVideo;

        //open.Filter = "Video File (*.mp4)";
        private OpenFileDialog open;

        public MainWindow()
        {
                InitializeComponent();
           
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                open = new OpenFileDialog { Filter = "Video File (*.mp4)|*.mp4;" };

                if (open.ShowDialog() != true) return;
                var video = new Video(open.FileName);
                returnedVideo = FingerprintManager.OpenVideo(video);
            })).Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.SplitWavFile(returnedVideo.FilePath, returnedVideo.FilePath);
        }

        public void WriteToForeground(string output)
        {
            fg_label.Content = output;
        }
    }
}