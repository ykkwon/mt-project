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
        private Media convertedMedia;
        private Media preprocessedMedia;
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
                    _open = new OpenFileDialog
                    {
                        Filter = "Video File (*.mp4)|*.mp4;",
                    };
                    _open.ShowDialog();

                    Media media = new Media(_open.FileName);
                    Console.WriteLine("File path: " + media.FilePath);
                    Console.WriteLine("File name: " + media.FileName);
                    Console.WriteLine("Converting input to Wave format.");
                    // Use the same file path as input media, to write to the same folder. 
                    var outputMedia = new Media(_open.FileName);
                    convertedMedia = FingerprintManager.FileToWav(media, outputMedia);
                    preprocessedMedia = FingerprintManager.Preprocess(convertedMedia);

                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    MainWindow.Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                FingerprintManager.SendToDatabase(entryName);
            })).Start();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FingerprintManager.SplitWavFile(preprocessedMedia.FilePath, convertedMedia.FilePath);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
                MainWindow.Main.Status = "Choose an input file to preprocess first.";
            }

        }

        internal string Status
        {
            get { return fg_label.Content.ToString(); }
            set
            {
                Dispatcher.Invoke(new Action(() =>
          {
              fg_label.Content += value + "\n";
          }));
            }
        }

        private void purgebutton_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.DeleteTable();
            MainWindow.Main.Status = "Table has been deleted. Don't forget to create it again.";
        }

        private void createbutton_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.CreateTable();
            MainWindow.Main.Status = "Table has been created.";
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
        }

        private void spectrogramButton_Click(object sender, RoutedEventArgs e)
        {
            try { 
            FingerprintManager.plotSpectrogram(preprocessedMedia.FilePath);
            }
            catch(NullReferenceException)
            {
                Console.WriteLine("No file loaded.");
            }
        }
    }
}