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
        internal static MainWindow Main;

        public MainWindow()
        {
            Main = this;
            InitializeComponent();
        }

        private Video _returnedVideo;
        private OpenFileDialog _open;

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                _open = new OpenFileDialog { Filter = "Video File (*.mp4)|*.mp4;" };

                if (_open.ShowDialog() != true) return;
                var video = new Video(_open.FileName);
                _returnedVideo = FingerprintManager.OpenVideo(video);
            })).Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.SendToDatabase();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.SplitWavFile(_returnedVideo.FilePath, _returnedVideo.FilePath);
        }

        internal string Status
        {
            get { return fg_label.Content.ToString(); }
            set { Dispatcher.Invoke(new Action(() =>
            {
                fg_label.Content += value + "\n";
            })); }
        }
    }
}