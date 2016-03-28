using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides;
using dbApp;
using Microsoft.Win32;

namespace DatabasePopulationApplication_0._4._5
{
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
            var nameDialog = new Popup_Title();
            if (nameDialog.ShowDialog() == true)
            {
                _entryName = nameDialog.ResponseText;
            }
            else
            {
                return;
            }

            var typeDialog = new Popup_Type();
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
                    Main.Status = "The file will appear in the database as:";
                    Main.Status = "Name: " + _entryName;
                    Main.Status = "Type: " + _typeName;
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
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg", //Resources.FileFilterJPeg,
                FileName = Path.GetFileNameWithoutExtension(filename) + "_fingerprints" + ".jpg"
            };

            sfd.ShowDialog();

            using (IAudio proxy = new BassProxy())
            {
                Main.Status = "Visualizing hashes. This might take a while, depending on the movie length.";
                Fingerprinter manager = new Fingerprinter();
                // Stridesize is length of fingerprint in bytes(68% sure)
                int strideSize = 1102;
                int samplesPerFingerprint = 128*64; // 128 = width of fingerprint, 64 = overlap
                var stride = new IncrementalStaticStride(strideSize, samplesPerFingerprint);


                //List<bool[]> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                //Console.WriteLine("Preliminary: " + preliminaryFingerprints.Count + " ---- " + test[1].HashBins[1]);

                //manager.GetHashSimilarity(stride, stride, proxy, filename, filename);
                int width = manager.FingerprintLength;
                int height = manager.LogBins;
                Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);

                ///////////////////////////////////////////////////////////////////////////////////////////
                string secondfile = "C:\\Users\\Kristian\\Desktop\\Bachelor Stuff\\Songlist\\Tale 011.m4a";
                var fingerprints2 = manager.CreateFingerprints(proxy, secondfile, stride);
                var test = manager.GetFingerHashes(stride, fingerprints);
                var test2 = manager.GetFingerHashes(stride, fingerprints2);

                // NOTE TO SELF: We should split up fingerprints of movie into different lists, 
                // ie. fingerprints from timestamp 0 - 600 seconds (10min) goes in one list, next 600seconds go to next list.
                // This is for faster comparing when we search later on.

                // sends in two lists of HashedFingerprints, returns timestamp if they match
                // Assuming first list is a section of fingerprints from the movie (say a list of fingerprints for 10minutes)
                var results = manager.GetTimeStamps(test, test2);
                if (results != -1)
                {
                    // do something with timestamp
                    int x = 0;
                } else {
                    // List of fingerprints were not a match
                    int x = 0;
                }
                var breakpointchecker = 0;
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
                FileName = Path.GetFileNameWithoutExtension(filename) + "_spectrum" + ".jpg"
            };

            int width = 1000;
            int height = 800;

            sfd.ShowDialog();
            using (BassProxy proxy = new BassProxy())
            {
                Main.Status = "Generating spectrogram visualization.";
                Fingerprinter manager = new Fingerprinter();

                float[][] data = manager.CreateSpectrogram(proxy, Path.GetFullPath(filename), 0, 0);
                Bitmap image = Imaging.GetSpectrogramImage(data, width, height);
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            }

        }

        private void drawWavelets()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg",
                FileName = Path.GetFileNameWithoutExtension(filename) + "_wavelets" + ".jpg"
            };
            sfd.ShowDialog();
            string path = Path.GetFullPath(sfd.FileName);
            using (IAudio proxy = new BassProxy())
            {
                Main.Status = "Generating wavelet visualization.";
                Fingerprinter manager = new Fingerprinter();

                StaticStride stride = new StaticStride((int)1102);
                Image image = Imaging.GetWaveletSpectralImage(Path.GetFullPath(filename), stride, proxy, manager);
                image.Save(path);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
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
                    var test = manager.GetFingerHashes(stride, fingerprints);
                    Main.Status = "Sending hashes to database. This might take a long time, depending on the movie length.";
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
                (new Thread(() =>
                {
                    Main.Status = "Truncating table.";
                    fdbm.truncateTable();
                    Main.Status = "Table successfully truncated.";
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