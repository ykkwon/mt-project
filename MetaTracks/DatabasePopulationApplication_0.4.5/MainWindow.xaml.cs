using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Windows;
using AcousticFingerprintingLibrary_0._4._5;
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

        int foregroundLabelCounter = 0;
        internal static MainWindow Main;
        private readonly FingerprintDatabaseManager _fdbm = new FingerprintDatabaseManager();
        private string _entryName;
        private string _typeName;
        private string _tmdbId;
        private string _filename;
        private bool _fileopened;
        private string csvFileName;

        private void sendFullCSVFile_Click(object sender, RoutedEventArgs e)
        {
            (new Thread(() =>
            {
                try
                {
                    OpenFileDialog ofdcsv = new OpenFileDialog();
                    if (ofdcsv.ShowDialog() == true)
                    {
                        _fileopened = true;
                        csvFileName = ofdcsv.FileName;
                        Main.Status = "Opened file: " + _filename;
                        Main.Status = "Writing to MySQL.";
                        _fdbm.WriteToMySql(csvFileName);
                        Main.Status = "Done.";
                    }
                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
            foregroundLabelCounter += 4;
        }    

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var nameDialog = new Popup_Title();
            if (nameDialog.ShowDialog() == true)
            {
                _entryName = nameDialog.ResponseText;
            }
            else {
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

            var idDialog = new Popup_Title();
            idDialog.textBlock.Text = "Write in the movie database ID for the movie.";
            if (idDialog.ShowDialog() == true)
            {
                _tmdbId = idDialog.ResponseText;
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
                    if (ofd.ShowDialog() == true)
                    {
                        _fileopened = true;
                        _filename = ofd.FileName;
                        Main.Status = "Opened file: " + _filename;
                        Main.Status = "The file will appear in the database as:";
                        Main.Status = "Name: " + _entryName;
                        Main.Status = "Type: " + _typeName;
                        Main.Status = "The Movie Database ID: " + _tmdbId;
                    }
                }
                catch (TypeInitializationException exception)
                {
                    Console.WriteLine(exception);
                    Main.Status = "Not connected to database. Connect through AWS Explorer.";
                }
            })).Start();
            foregroundLabelCounter += 4;
        }

        private void DrawFingerprints(SaveFileDialog sfd)
        {
            Main.Status = "Visualizing hashes. This might take a while, depending on the movie length.";
            AcousticFingerprintingLibrary_0._4._5.FingerprintManager manager =
                new AcousticFingerprintingLibrary_0._4._5.FingerprintManager();
            // DistanceeSize is length between consecutive fingerprints
            int distanceSize = 1102;
            int samplesPerFingerprint = 128 * 64; // 128 = width of fingerprint, 64 = overlap


            //List<bool[]> fingerprints = manager.CreateFingerprints(proxy, Path.GetFullPath(filename), Distance);
            List<Fingerprint> fingerprints = manager.CreateFingerprints(Path.GetFullPath(_filename));

            //manager.GetHashSimilarity(Distance, Distance, proxy, filename, filename);
            int width = manager.FingerprintWidth;
            int height = manager.LogBins;
            Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
            image.Save(sfd.FileName, ImageFormat.Jpeg);
            image.Dispose();
            Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            foregroundLabelCounter += 2;
        }

        private void DrawSpectrogram(SaveFileDialog sfd)
        {
            const int width = 1000;
            const int height = 800;

            Main.Status = "Generating spectrogram visualization.";
            FingerprintManager manager =
                new FingerprintManager();

            float[][] data = manager.CreateImageSpectrogram(Path.GetFullPath(_filename), 0, 0);
            Bitmap image = Imaging.GetSpectrogramImage(data, width, height);
            image.Save(sfd.FileName, ImageFormat.Jpeg);
            image.Dispose();
            Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
            foregroundLabelCounter += 2;

        }

        private void DrawWavelets(SaveFileDialog sfd)
        {
            string path = Path.GetFullPath(sfd.FileName);
            using (BassProxy proxy = new BassProxy())
            {
                Main.Status = "Generating wavelet visualization.";
                FingerprintManager manager = new FingerprintManager();
                
                Image image = Imaging.GetWaveletSpectralImage(Path.GetFullPath(_filename), proxy, manager);
                image.Save(path);
                image.Dispose();
                Main.Status = "Visualization done. Image file saved to: " + Path.GetFullPath(sfd.FileName);
                foregroundLabelCounter += 2;
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            (new Thread(() =>
            {

                FingerprintManager manager = new FingerprintManager();
                try
                {
                    if (_fileopened != true)
                    {
                        throw new ArgumentNullException();
                    }
                    Main.Status = "Staging the following file: " + _filename;
                    List<Fingerprint> fingerprints = manager.CreateFingerprints(Path.GetFullPath(_filename));

                    var test = manager.GetFingerHashes(fingerprints);
                    Main.Status = "Sending hashes to database. This might take a long time, depending on the movie length.";

                    var csv = new StringBuilder();
                    foreach (var fingerprint in test)
                    {
                        if (fingerprint.SequenceNumber % (test.Length / 100) == 0)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                           {
                               ProgressBar.Value += 1;

                           }));
                        }

                        for (int i = 0; i < fingerprint.HashBins.Length; i++)
                        {

                            var currentHash = fingerprint.HashBins[i];
                            //
                            var naught = 0;
                            var first = _entryName;
                            var second = fingerprint.Timestamp;
                            var third = fingerprint.SequenceNumber;
                            var fourth = currentHash;
                            var fifth = _typeName;
                            var sixth = _tmdbId;
                            var newLine = string.Format("{0};{1};{2};{3};{4};{5};{6}", naught, first, second, third, fourth, fifth, sixth);
                            csv.AppendLine(newLine);
                        }
                    }
                    File.WriteAllText(Path.GetTempPath() + _entryName + "fingerprints.csv", csv.ToString());
                    Console.WriteLine(Path.GetTempPath());



                    Main.Status = "Printed CSV file to: " + Path.GetTempPath() + _entryName + "fingerprints.csv";
                    Main.Status = "Wrapping up. . .";
                    _fdbm.WriteToMySql(Path.GetTempPath() + _entryName + "fingerprints.csv");
                    Main.Status = "Done.";
                    foregroundLabelCounter += 5;
                    Main.Status = "Elapsed time preprocessing and sending: " + stopWatch.Elapsed;
                    stopWatch.Stop();
                }
                catch (ArgumentNullException)
                {
                    Main.Status = "You need to preprocess a file first.";
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
                    foregroundLabelCounter++;
                    if (foregroundLabelCounter >= 20)
                    {
                        fg_label.Content = "";
                        fg_label.Content += value + "\n";
                        foregroundLabelCounter = 0;
                    }
                });
            }
        }


        private void purgebutton_Click(object sender, RoutedEventArgs e)
        {

            Popup_Confirmation dialog = new Popup_Confirmation();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
                (new Thread(() =>
                {
                    Main.Status = "Truncating table.";
                    _fdbm.TruncateTable();
                    Main.Status = "Table successfully truncated.";
                })).Start();
        }

        private void spectrogramButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fileopened)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "(*.jpg)|*.jpg", //Resources.FileFilterJPeg,
                    FileName = Path.GetFileNameWithoutExtension(_filename) + "_spectrum" + ".jpg"
                };
                if (sfd.ShowDialog() == true)
                {
                    (new Thread(() =>
                    {
                        try
                        {
                            DrawSpectrogram(sfd);
                        }
                        catch (NullReferenceException)
                        {
                            Console.WriteLine(@"No file loaded.");
                        }
                    })).Start();
                }
            }
            else Main.Status = "You need to preprocess a file first.";
        }

        private void waveletButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fileopened)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "(*.jpg)|*.jpg",
                    FileName = Path.GetFileNameWithoutExtension(_filename) + "_wavelets" + ".jpg"
                };
                if (sfd.ShowDialog() == true)
                {
                    (new Thread(() =>
                    {
                        DrawWavelets(sfd);
                    })).Start();
                }
            }
            else Main.Status = "You need to preprocess a file first.";
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hashButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fileopened)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "(*.jpg)|*.jpg",
                    FileName = Path.GetFileNameWithoutExtension(_filename) + "_fingerprints" + ".jpg"
                };
                if (sfd.ShowDialog() == true)
                {
                    (new Thread(() =>
                    {
                        DrawFingerprints(sfd);
                    })).Start();
                }
            }
            else Main.Status = "You need to preprocess a file first.";
        }

        private void compareButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fileopened)
            {
                string titleInput = null;
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
                        FingerprintManager manager =
                            new FingerprintManager();


                        if (titleInput != null)
                        {
                            string address =
                                "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle=" +
                                $"'{Uri.EscapeDataString(titleInput)}'";
                            var client = new HttpClient { BaseAddress = new Uri(address) };
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(
                                new MediaTypeWithQualityHeaderValue("application/json"));

                            // HTTP GET
                            HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                            var responseString = response.Content.ReadAsStringAsync().Result;
                            var receivedHashes = responseString.Split(';');
                            Console.WriteLine(@"Got all hashes.");

                            string[] receivedtime = new string[receivedHashes.Length];

                            var fingerprints2 = manager.CreateFingerprints(_filename);
                            
                            var toCompare = manager.GetFingerHashes(fingerprints2);
                            var movie = manager.GenerateHashedFingerprints(receivedHashes, receivedtime);

                            // Splits the list of fingerprints into smaller chunks, for faster searching
                            ///var newList = manager.SplitFingerprintLists(movie);
                            /*foreach (var movieChunk in newList)
                            {
                                result += manager.CompareFingerprintListsHighest(movieChunk, toCompare);
                            }*/

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
            else Main.Status = "You need to preprocess a file first.";
        }
    }
}
