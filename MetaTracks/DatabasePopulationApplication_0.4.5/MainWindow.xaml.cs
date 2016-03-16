using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using AcousticFingerprintingLibrary;
using AcousticFingerprintingLibrary.SoundFingerprint;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies.Strides;
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
        private string filename;
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
                    /*
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
                    */

                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.ShowDialog();
                    filename = ofd.FileName;

                    drawSpectrogram();
                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    //Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
        }

        private void drawSpectrogram()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg", //Resources.FileFilterJPeg,
                FileName = Path.GetFileNameWithoutExtension(filename) + "_spectrum_" + ".jpg"
            };
            int width = 1000;
            int height = 800;
            //
            sfd.ShowDialog();
            using (BassProxy proxy = new BassProxy())
            {
                Fingerprinter manager = new Fingerprinter();

                float[][] data = manager.CreateSpectrogram(proxy, Path.GetFullPath(filename), 0, 0);
                Bitmap image = Imaging.GetSpectrogramImage(data, width, height);
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
            }
            //
        }

        private void drawWavelets()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg", //Resources.FileFilterJPeg,
                FileName = Path.GetFileNameWithoutExtension(filename) + "_wavelets_" + ".jpg"
            };
            sfd.ShowDialog();
            string path = Path.GetFullPath(sfd.FileName);
            using (IAudio proxy = new BassProxy())
            {
                Fingerprinter manager = new Fingerprinter();
                // 1102?
                StaticStride stride = new StaticStride((int)1102);
                Image image = Imaging.GetWaveletSpectralImage(Path.GetFullPath(filename), stride, proxy, manager);
                image.Save(path);
                image.Dispose();
            }
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
            drawWavelets();
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