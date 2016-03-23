using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using AcousticFingerprintingLibrary_0._4._5;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides;
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
        //FingerprintDatabaseManager fdm = new FingerprintDatabaseManager();
        private string _entryName;
        private string _typeName;
        private OpenFileDialog _open;
        private string filename;
        private Media _convertedMedia;
        private Media _preprocessedMedia;

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var nameDialog = new Popup();
            if (nameDialog.ShowDialog() == true)
            {
                _entryName = nameDialog.ResponseText;
            }
            else
            {
                return;
            }

            var typeDialog = new Popup();
            if (typeDialog.ShowDialog() == true)
            {
                _typeName = typeDialog.ResponseText;
            }
            else
            {
                return;
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
                    Console.WriteLine(@"File path: " + inputMedia.FilePath);
                    Console.WriteLine(@"File name: " + inputMedia.FileName);
                    Console.WriteLine(@"Converting input to Wave format.");
                    _convertedMedia = fm.ConvertToWav(inputMedia);
                    _preprocessedMedia = fm.Preprocess(_convertedMedia, 5512);


                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.ShowDialog();
                    filename = ofd.FileName;
                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
        }

        private void DrawFingerprints()
        {
            if (true)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "(*.jpg)|*.jpg",
                    FileName = Path.GetFileNameWithoutExtension(filename) + "_fingerprint_" + ".jpg"
                };

                sfd.ShowDialog();
                string path = Path.GetFullPath(sfd.FileName);
                using (IAudio proxy = new BassProxy())
                {
                    Fingerprinter manager = new Fingerprinter();
                    // Not 100% sure what stridenumber means
                    int strideSize = 1102;
                    int samplesPerFingerprint = 128*64; // 128 = width of fingerprint, 64 = overlap
                    var stride = new IncrementalStaticStride(strideSize, samplesPerFingerprint);

                    int totalFingerprints = 0;

                    //List<bool[]> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                    List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                    //manager.GetHashSimilarity(stride, stride, proxy, filename, "C:\\Users\\Kristian\\Desktop\\c# musikkfile\\Pokemon_BlueRed_-_Route_1.wav");
                    var test = manager.GetFingerHashes(stride, fingerprints, path);
                    int width = manager.FingerprintLength;
                    int height = manager.LogBins;
                    Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
                    image.Save(path);
                    image.Dispose();
                }
            }
            // if each picture is in own file
            /*
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    string fileName = Path.GetFileName(_tbPathToFile.Text);
                    FadeControls(false);
                    Action action = () =>
                    {
                        using (IAudio proxy = new BassProxy())
                        {
                            FingerprintManager manager = new FingerprintManager();
                            StaticStride stride = new StaticStride((int)_nudStride.Value);
                            List<bool[]> result = manager.CreateFingerprints(proxy, Path.GetFullPath(_tbPathToFile.Text), stride);
                            int i = -1;
                            int width = manager.FingerprintLength;
                            int height = manager.LogBins;
                            foreach (bool[] item in result)
                            {
                                Image image = Imaging.GetFingerprintImage(item, width, height);
                                image.Save(path + "\\" + fileName + i++ + ".jpg", ImageFormat.Jpeg);
                            }
                        }
                    };
                    action.BeginInvoke((result) =>
                    {
                        FadeControls(true);
                        MessageBox.Show(Resources.ImageIsDrawn, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        action.EndInvoke(result);
                    }
                        , action);
                }
            }*/
        }

        private string Selector(bool itemin)
        {
            return itemin.ToString();
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
                StaticStride stride = new StaticStride((int) 1102);
                Image image = Imaging.GetWaveletSpectralImage(Path.GetFullPath(filename), stride, proxy, manager);
                image.Save(path);
                image.Dispose();
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                Main.Status = "Not implemented.";
                //fdm.SendFullFileToDatabase(_entryName);
            })).Start();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                try
                {
                    Main.Status = "Not implemented.";
                    //_splitVideosList = FingerprintManager.SplitWavFile(_preprocessedMedia, _preprocessedMedia);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex);
                    // Main.Status = "Choose an input file to preprocess first.";
                }
            })).Start();

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

            Confirmation dialog = new Confirmation();
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                Main.Status = "Purging database.";
            }
            if (dialog.DialogResult == false)
            {
                Main.Status = "I am glad to see that you changed your mind.";
            }
            //FingerprintManager.DeleteTable();
            //Main.Status = "Table has been deleted. Don't forget to create it again.";
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
                    drawSpectrogram();
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
            DrawFingerprints();
            //throw new NotImplementedException();
            //FingerprintManager.HashTransform(_splitVideosList);
        }
    }
}