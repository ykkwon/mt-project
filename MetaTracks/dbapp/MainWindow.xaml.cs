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
        public MainWindow()
        {
            Main = this;
            InitializeComponent();
        }

        internal static MainWindow Main;
        private Video _returnedVideo;
        private OpenFileDialog _open;
        private string entryName; 

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Popup();
            if (dialog.ShowDialog() == true)
            {
                entryName = dialog.ResponseText;
            }

            (new Thread(() =>
            {
                try
                {
                    _open = new OpenFileDialog {Filter = "Video File (*.mp4)|*.mp4;"};

                    if (_open.ShowDialog() != true) return;
                    Video video = new Video(_open.FileName);
                    _returnedVideo = FingerprintManager.OpenVideo(video);
                }
                catch (TypeInitializationException exception)
                {
                    MainWindow.Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            
            FingerprintManager.SendToDatabase(entryName);
            Console.WriteLine("Finished");
            MainWindow.Main.Status = "Movie has been added to database successfully.";

        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FingerprintManager.SplitWavFile(_returnedVideo.FilePath, _returnedVideo.FilePath);
            }
            catch (NullReferenceException ex)
            {
                MainWindow.Main.Status = "Choose an input file to preprocess first.";
            }
            
        }

        internal string Status
        {
            get { return fg_label.Content.ToString(); }
            set { Dispatcher.Invoke(new Action(() =>
            {
                fg_label.Content += value + "\n";
            })); }
        }

        private void purgebutton_Click(object sender, RoutedEventArgs e)
        {
            //FingerprintManager.DeleteTable();
            FingerprintManager.CreateTable();
        }
    }
}