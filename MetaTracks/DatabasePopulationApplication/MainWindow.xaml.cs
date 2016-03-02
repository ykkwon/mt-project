using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using dbApp.Fingerprint;
using System.Threading;

namespace dbApp
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
        private Media _convertedMedia;
        private Media _preprocessedMedia;
        private OpenFileDialog _open;
        private string _entryName;
        private List<string> _splitVideosList; 

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

                    Media media = new Media(_open.FileName);
                    Console.WriteLine("File path: " + media.FilePath);
                    Console.WriteLine("File name: " + media.FileName);
                    Console.WriteLine("Converting input to Wave format.");
                    // Use the same file path as input media, to write to the same folder. 
                    var outputMedia = new Media(_open.FileName);
                    _convertedMedia = FingerprintManager.FileToWav(media, outputMedia);
                    _preprocessedMedia = FingerprintManager.Preprocess(_convertedMedia);

                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                FingerprintManager.SendToDatabase(_entryName);
            })).Start();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                try
            {
                _splitVideosList = FingerprintManager.SplitWavFile(_preprocessedMedia, _preprocessedMedia);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
                Main.Status = "Choose an input file to preprocess first.";
            }})).Start();

        }

        internal string Status
        {
            get { return fg_label.Content.ToString(); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    fg_label.Content += value + "\n";
                });
            }
        }

        private void purgebutton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                FingerprintManager.DeleteTable();
                Main.Status = "Table has been deleted. Don't forget to create it again.";
            })).Start();
        }

        private void createbutton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
            FingerprintManager.CreateTable();
            Main.Status = "Table has been created.";
            })).Start();
        }

        private void spectrogramButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                try { 
            FingerprintManager.PlotSpectrogram(_preprocessedMedia.FilePath);
            }
            catch(NullReferenceException)
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
            FingerprintManager.HashTransform(_splitVideosList);
        }
    }
}   