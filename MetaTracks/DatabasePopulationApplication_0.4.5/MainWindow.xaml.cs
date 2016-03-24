using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
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
        FingerprintManager fm = new FingerprintManager();
        FingerprintDatabaseManager fdbm = new FingerprintDatabaseManager();
        private string _entryName;
        private string _typeName;
        private string filename;

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
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.ShowDialog();
                    filename = ofd.FileName;
                    Main.Status = "Opened file: " + filename;
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
            using (IAudio proxy = new BassProxy())
            {
                Fingerprinter manager = new Fingerprinter();
                // Not 100% sure what stridenumber means
                int strideSize = 1102;
                int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                var stride = new IncrementalStaticStride(strideSize, samplesPerFingerprint);

                int totalFingerprints = 0;

                //List<bool[]> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                var test = manager.GetFingerHashes(stride, fingerprints, null);
                //Console.WriteLine("Preliminary: " + preliminaryFingerprints.Count + " ---- " + test[1].HashBins[1]);
                
                manager.GetHashSimilarity(stride, stride, proxy, filename, filename);
                int width = manager.FingerprintLength;
                int height = manager.LogBins;
                Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
                image.Save(Path.GetFullPath(filename));
                image.Dispose();
            }
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
            Main.Status = "Staging the following file: " + filename;
            (new Thread(() =>
            {
                using (IAudio proxy = new BassProxy())
                {
                    Fingerprinter manager = new Fingerprinter();
                    int strideSize = 1102;
                    int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                    var stride = new IncrementalStaticStride(strideSize, samplesPerFingerprint);

                    List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                    var test = manager.GetFingerHashes(stride, fingerprints, null);
                    Main.Status = "Sending hashes to database. This might take a long time. ";
                    foreach (var fingerprint in test)
                    {
                        var currentHash = fingerprint.HashBins[1];
                        fdbm.insertFingerprints(_entryName, fingerprint.Timestamp, fingerprint.SequenceNumber, currentHash);
                    }
                    Console.WriteLine("Done");
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

            //FingerprintManager.DeleteTable();
            //Main.Status = "Table has been deleted. Don't forget to create it again.";
        }
    

        private void createbutton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                Main.Status = "Table has been created.";
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
            (new Thread(() =>
            {
                drawWavelets();
            })).Start();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hashButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                DrawFingerprints();
            })).Start();
        }
    }

}