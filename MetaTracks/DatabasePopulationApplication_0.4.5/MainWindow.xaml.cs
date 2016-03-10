using System;
using System.Threading;
using System.Windows;
using AcousticFingerprintingLibrary;
using dbApp;
using Microsoft.Win32;

namespace DatabasePopulationApplication_0._4._5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Main = this;
            InitializeComponent();
            MouseDown += delegate { DragMove(); };
        }

        internal static MainWindow Main;
        // From acoustic fingerprinting client
        FingerprintManager fm = new FingerprintManager();
        private string _entryName;
        private OpenFileDialog _open;
        private Media _convertedMedia;
        private Media _preprocessedMedia;

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Popup();
            if (dialog.ShowDialog() == true)
            {
                _entryName = dialog.ResponseText;
            }

            (new Thread(() =>
            {
                try
                {
                    _open = new OpenFileDialog
                    {
                        Filter = "MP4 Video File (*.mp4)|*.mp4;|AVI Video File (*.avi)|*.avi;|All files (*.*)|(*.*);",
                    };
                    _open.ShowDialog();

                    var inputMedia = new Media(_open.FileName);
                    Console.WriteLine("File path: " + inputMedia.FilePath);
                    Console.WriteLine("File name: " + inputMedia.FileName);
                    Console.WriteLine("Converting input to Wave format.");
                    _convertedMedia = fm.ConvertToWav(inputMedia);
                    _preprocessedMedia = fm.Preprocess(_convertedMedia, 5512);

                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    //Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                throw new NotImplementedException();
                //fm.SendToDatabase(_entryName);
            })).Start();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                try
                {
                    throw new NotImplementedException();
                    //_splitVideosList = FingerprintManager.SplitWavFile(_preprocessedMedia, _preprocessedMedia);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex);
                   // Main.Status = "Choose an input file to preprocess first.";
                }
            })).Start();

        }

        /**
        internal string Status
        {
            get { return fg_label.Content.ToString(); }
            set
            {
                System.Windows.Threading.Dispatcher.Invoke(() =>
                {
                    fg_label.Content += value + "\n";
                });
            }
        }
    **/

        private void purgebutton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {

                throw new NotImplementedException();
                //FingerprintManager.DeleteTable();
                //Main.Status = "Table has been deleted. Don't forget to create it again.";
            })).Start();
        }

        private void createbutton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                //FingerprintManager.CreateTable();
                //Main.Status = "Table has been created.";
            })).Start();
        }

        private void spectrogramButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                try
                {
                    throw new NotImplementedException();
                    //FingerprintManager.PlotSpectrogram(_splitVideosList, _preprocessedMedia.DirPath);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("No file loaded.");
                }
            })).Start();
        }

        private void waveletButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hashButton_Click(object sender, RoutedEventArgs e)
        {

            throw new NotImplementedException();
            //FingerprintManager.HashTransform(_splitVideosList);
        }
    }
}