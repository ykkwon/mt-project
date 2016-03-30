using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private async void DrawFingerprints()
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
                int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                var stride = new IncrementalStaticStride(strideSize, samplesPerFingerprint);


                //List<bool[]> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);
                List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), stride);

                //manager.GetHashSimilarity(stride, stride, proxy, filename, filename);
                int width = manager.FingerprintLength;
                int height = manager.LogBins;
                Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            }
        }
        public HashedFingerprint[] GenerateHashedFingerprints(string[] receivedHashes, string[] receivedTimestamps)
        {
            List<long> hashBins = new List<long>();
            List<double> timestamps = new List<double>();

            List<HashedFingerprint> receivedFingerprints = new List<HashedFingerprint>();

            for (int index = 0; index < receivedHashes.Length - 1; index++)
            {
                hashBins.Add(Convert.ToInt64(receivedHashes[index]));
                timestamps.Add(Convert.ToDouble(receivedTimestamps[index]));
            }
            List<long[]> hashBinsList = new List<long[]>();
            List<double> timestampList = new List<double>();
            var indexer = 0;
            for (int j = 0; j < timestamps.Count - 1; j++)
            {
                if (j % 20 == 0 && hashBins.Count > j + 20)
                {
                    long[] bins = new long[20];
                    for (int i = 0; i < 20; i++)
                    {
                        bins[i] = hashBins[i + j];
                    }
                    hashBinsList.Add(bins);
                    timestampList.Add(timestamps[j]);
                    indexer += bins.Length;
                }
            }
            for (int i = 0; i < hashBinsList.Count; i++)
            {
                var finger = new HashedFingerprint(hashBinsList[i], timestampList[i]);
                receivedFingerprints.Add(finger);

            }

            return receivedFingerprints.ToArray();
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
                        for (int i = 0; i < 19; i++)
                        {
                            var currentHash = fingerprint.HashBins[i];
                            fdbm.insertFingerprints(_entryName, fingerprint.Timestamp, fingerprint.SequenceNumber, currentHash);
                        }
                    }
                    Main.Status = "Done.";
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

        private async void compareButton_Click(object sender, RoutedEventArgs e)
        {

            using (IAudio proxy = new BassProxy())
            {
                string secondFile = null;
                string titleInput = null;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.ShowDialog();
                secondFile = ofd.FileName;
                Main.Status = "Comparing chosen digital file with fingerprints from database.";
                Fingerprinter manager = new Fingerprinter();
                int strideSize = 1102;
                int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                var stride = new IncrementalStaticStride(strideSize, samplesPerFingerprint);

                var nameDialog2 = new Popup_Title();
                if (nameDialog2.ShowDialog() == true)
                {
                    titleInput = nameDialog2.ResponseText;
                    
                }

                string address = string.Format("http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle=" + "'{0}'",
                    Uri.EscapeDataString(titleInput));
                var client = new HttpClient();
                client.BaseAddress = new Uri(address);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                var receivedHashes = responseString.Split(',');
                Console.WriteLine("Got all hashes.");
                List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(secondFile), stride);

                string[] receivedtime = new string[receivedHashes.Length];

                var fingerprints2 = manager.CreateFingerprints(proxy, secondFile, stride);
                var movie = GenerateHashedFingerprints(receivedHashes, receivedtime);


                var toCompare = manager.GetFingerHashes(stride, fingerprints2);


                // NOTE TO SELF: We should split up fingerprints of movie into different lists, 
                // ie. fingerprints from timestamp 0 - 600 seconds (10min) goes in one list, next 600seconds go to next list.
                // This is for faster comparing when we search later on.

                // sends in two lists of HashedFingerprints, returns timestamp if they match
                // Assuming first list is a section of fingerprints from the movie (say a list of fingerprints for 10minutes)
                var results = manager.GetTimeStamps(movie, toCompare);
                var totalMatch = manager.CompareFingerprintListsHighest(movie, toCompare);
                Main.Status = "RESULT: " + totalMatch;
                
            }
        }
    }
}
