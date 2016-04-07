using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Windows;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Distance;
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
            MouseLeftButtonDown += delegate { DragMove(); };
        }

        internal static MainWindow Main;
        private FingerprintDatabaseManager fdbm = new FingerprintDatabaseManager();
        private string _entryName;
        private string _typeName;
        private string _filename;

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
                    _filename = ofd.FileName;
                    Main.Status = "Opened file: " + _filename;
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
                FileName = Path.GetFileNameWithoutExtension(_filename) + "_fingerprints" + ".jpg"
            };

            sfd.ShowDialog();

            using (BassProxy proxy = new BassProxy())
            {
                Main.Status = "Visualizing hashes. This might take a while, depending on the movie length.";
                Fingerprinter manager = new Fingerprinter();
                // DistanceeSize is length of fingerprint in bits
                int distanceSize = 1102;
                int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                var distance = new IncrementalDistance(distanceSize, samplesPerFingerprint);


                //List<bool[]> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), Distance);
                List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(_filename), distance);

                //manager.GetHashSimilarity(Distance, Distance, proxy, filename, filename);
                int width = manager.FingerprintLength;
                int height = manager.LogBins;
                Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            }
        }

        private void DrawSpectrogram()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg", //Resources.FileFilterJPeg,
                FileName = Path.GetFileNameWithoutExtension(_filename) + "_spectrum" + ".jpg"
            };

            int width = 1000;
            int height = 800;

            sfd.ShowDialog();
            using (BassProxy proxy = new BassProxy())
            {
                Main.Status = "Generating spectrogram visualization.";
                Fingerprinter manager = new Fingerprinter();

                float[][] data = manager.CreateSpectrogram(proxy, Path.GetFullPath(_filename), 0, 0);
                Bitmap image = Imaging.GetSpectrogramImage(data, width, height);
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            }

        }

        private void DrawWavelets()
        {

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg",
                FileName = Path.GetFileNameWithoutExtension(_filename) + "_wavelets" + ".jpg"
            };

            sfd.ShowDialog();
            string path = Path.GetFullPath(sfd.FileName);
            using (BassProxy proxy = new BassProxy())
            {
                Main.Status = "Generating wavelet visualization.";
                Fingerprinter manager = new Fingerprinter();

                Distance distance = new Distance(1102);
                Image image = Imaging.GetWaveletSpectralImage(Path.GetFullPath(_filename), distance, proxy, manager);
                image.Save(path);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Status = "Staging the following file: " + _filename;
            (new Thread(() =>
            {
                using (BassProxy proxy = new BassProxy())
                {
                    Fingerprinter manager = new Fingerprinter();
                    int distanceSize = 1102;
                    int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                    var distance = new IncrementalDistance(distanceSize, samplesPerFingerprint);
                    try
                    {
                        List<Fingerprint> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(_filename),
                            distance);
                        var test = manager.GetFingerHashes(distance, fingerprints);
                        Main.Status = "Sending hashes to database. This might take a long time, depending on the movie length.";

                        var csv = new StringBuilder();
                        foreach (var fingerprint in test)
                        {
                            for (int i = 0; i < fingerprint.HashBins.Length; i++)
                            {
                                var currentHash = fingerprint.HashBins[i];
                                //
                                var first = _entryName;
                                var second = fingerprint.Timestamp;
                                var third = fingerprint.SequenceNumber;
                                var fourth = currentHash;
                                var newLine = string.Format("{0};{1};{2};{3}", first, second, third, fourth);
                                csv.AppendLine(newLine);

                                //fdbm.insertFingerprints(_entryName, fingerprint.Timestamp, fingerprint.SequenceNumber, currentHash);
                            }
                        }
                        File.WriteAllText(Path.GetTempPath() + _entryName + "fingerprints.csv", csv.ToString());
                        Console.WriteLine(Path.GetTempPath());
                       
                        

                        Main.Status = "Printed CSV file to: " + Path.GetTempPath() + _entryName + "fingerprints.csv";
                        // kjøre metoden ^^
                        fdbm.writeToMySQL(Path.GetTempPath() + _entryName + "fingerprints.csv");
                        Main.Status = "Done.";
                    }
                    catch(ArgumentNullException)
                    {
                        Main.Status = "You need to preprocess a file first.";
                    }
                    
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
                    DrawSpectrogram();
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(@"No file loaded.");
                }
            })).Start();
        }

        private void waveletButton_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                DrawWavelets();
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
            string titleInput = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var secondFile = ofd.FileName;
            var nameDialog2 = new Popup_Title();
            if (nameDialog2.ShowDialog() == true)
            {
                titleInput = nameDialog2.ResponseText;

            }

            (new Thread(async () =>
            {
                using (BassProxy proxy = new BassProxy())
            {
                
                Main.Status = "Comparing chosen digital file with fingerprints from database.";
                Fingerprinter manager = new Fingerprinter();
                int distanceSize = 1102;
                int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap
                var distance = new IncrementalDistance(distanceSize, samplesPerFingerprint);


                if (titleInput != null)
                {
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
                    Console.WriteLine(@"Got all hashes.");

                    string[] receivedtime = new string[receivedHashes.Length];

                    var fingerprints2 = manager.CreateFingerprints(proxy, secondFile, distance);


                    var toCompare = manager.GetFingerHashes(distance, fingerprints2);
                    var movie = manager.GenerateHashedFingerprints(receivedHashes, receivedtime, toCompare[0].HashBins.Length);


                    // NOTE TO SELF: We should split up fingerprints of movie into different lists, 
                    // ie. fingerprints from timestamp 0 - 600 seconds (10min) goes in one list, next 600seconds go to next list.
                    // This is for faster comparing when we search later on.

                    // sends in two lists of HashedFingerprints, returns timestamp if they match
                    // Assuming first list is a section of fingerprints from the movie (say a list of fingerprints for 10minutes)
                    //var results = manager.GetTimeStamps(movie, toCompare);
                    var totalMatch = manager.CompareFingerprintListsHighest(movie, toCompare);
                    Main.Status = "Percentage of matched fingerprints: " + totalMatch;
                }
            }
            })).Start();
        }
    }
}
