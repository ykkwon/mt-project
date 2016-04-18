using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using AVFoundation;
using Foundation;
using UIKit;
using System.Threading;
using System.Threading.Tasks;
using AcousticFingerprintingLibrary_0._4._5;
using CoreGraphics;
using AudioToolbox;

namespace iOSApplication_0._5._3
{
    /// <summary>
    /// Main View Controller for the iOS application.
    /// </summary>
    public partial class ViewController : UIViewController
    {
        private static List<HashedFingerprint[]> _hashedFingerprints;
        private static int _sampleRate;
        private static int _channels;
        private static AudioFormatType _audioFormat;
        private static AVAudioQuality _audioQuality;
        private string _selectedMovie;
        private UITableView _table;
        private string[] _availableMovies;
        private string[] _receivedHashes;
        private string[] _receivedTimestamps;
        private double matchCounter;


        /// <summary>
        /// The View Controller handle.
        /// </summary>
        /// <param name="handle"></param>
        public ViewController(IntPtr handle) : base(handle)
        {
            IndexMovies();
        }

        /// <summary>
        /// Fetch all available movies from the Web API.
        /// </summary>
        public async void IndexMovies()
        {
            var client = new HttpClient();
            const string titleQuery = @"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL";
            client.BaseAddress = new Uri(titleQuery);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage titleResponse = await client.GetAsync(client.BaseAddress);
            var responseString = titleResponse.Content.ReadAsStringAsync().Result;
            _availableMovies = responseString.Split(',');
            Array.Sort(_availableMovies);
            MoviePicker.Enabled = true;
        }

        /// <summary>
        /// Called after the controller’s <see cref="P:UIKit.UIViewController.View"/> is loaded into memory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called after <c>this</c> <see cref="T:UIKit.UIViewController"/>'s <see cref="P:UIKit.UIViewController.View"/> and its entire view hierarchy have been loaded into memory. This method is called whether the <see cref="T:UIKit.UIView"/> was loaded from a .xib file or programmatically.
        /// </para>
        /// </remarks>
        public override void ViewDidLoad()
        {
            RecordManager.Observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(RecordManager.OnDidPlayToEndTime);
           
            _sampleRate = 5512;
            _channels = 1;
            _audioFormat = AudioFormatType.LinearPCM;
            _audioQuality = AVAudioQuality.Max;

            base.ViewDidLoad();

            GetFingerprintsButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            RecordButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            StopButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            MoviePicker.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            LongRecordButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            SetButtonAvailability(false, false, false, false, false);
            
            // Event handler for simple "Record (short)" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {
                SetButtonAvailability(false, false, false, false, true);

                Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        var session = AVAudioSession.SharedInstance();
                        NSError error;
                        session.SetCategory(AVAudioSession.CategoryRecord, out error);
                        RecordManager.CreateOutputUrl(i);
                        RecordManager.PrepareAudioRecording(i, _sampleRate, _audioFormat, _channels, _audioQuality);
               
                        RecordManager.Recorder.Record();
                        Thread.Sleep(3000);
                        RecordManager.Recorder.Stop();
                        var currentWaveFile = RecordManager.AudioFilePath;
                        var consumedWaveFileShort = RecordManager.ConsumeWaveFileShort(currentWaveFile.RelativePath);
                        matchCounter += consumedWaveFileShort;

                        var internalMatchCounter = matchCounter;
                        InvokeOnMainThread(() =>
                        {
                            ForegroundLabel.Text = "Matched second: " + (3 + FingerprintManager.LatestTimeStamp) + " s" +
                                                   "\n" + internalMatchCounter + " fingerprints in total." + "\n" + "~ " +
                                                   (Math.Round(FingerprintManager.LatestTimeStamp/60)) + " minutes.";
                        });
                    }
                });
            };

            // Event handler for simple "Record (long)" button click and release.
            LongRecordButton.TouchUpInside += (sender, e) =>
            {
                    SetButtonAvailability(false, false, false, false, true);

                    var preSession = AVAudioSession.SharedInstance();
                    NSError preError;
                    preSession.SetCategory(AVAudioSession.CategoryRecord, out preError);
                    RecordManager.CreateOutputUrl(0);
                    RecordManager.PrepareAudioRecording(0, _sampleRate, _audioFormat, _channels, _audioQuality);
                    RecordManager.Recorder.Record();
                    Thread.Sleep(10000);
                    RecordManager.Recorder.Stop();
                    var prePath = RecordManager.AudioFilePath;
                    var result = RecordManager.ConsumeFirstFile(prePath.RelativePath, _hashedFingerprints);

                    Task.Factory.StartNew(() =>
                    {
                        InvokeOnMainThread(() =>
                        {
                            ForegroundLabel.Text = "Guessed chunk: " + result;
                        });

                        for (int i = 0; i < 1000; i++)
                        {
                            if (i % 20 == 0)
                            {
                                Console.WriteLine("Running long search. . .");
                                RecordManager.ConsumeFirstFile(prePath.RelativePath, _hashedFingerprints);
                            }

                            var session = AVAudioSession.SharedInstance();
                            NSError error;
                            session.SetCategory(AVAudioSession.CategoryRecord, out error);
                            RecordManager.CreateOutputUrl(i);
                            RecordManager.PrepareAudioRecording(i, _sampleRate, _audioFormat, _channels, _audioQuality);
                            RecordManager.Recorder.Record();
                            Thread.Sleep(3000);
                            RecordManager.Recorder.Stop();
                            var currentWaveFile = RecordManager.AudioFilePath;
                            var consumedWaveFileShort = RecordManager.ConsumeWaveFile(currentWaveFile.RelativePath, result);
                            matchCounter += consumedWaveFileShort;

                            var internalMatchCounter = matchCounter;
                            InvokeOnMainThread(() =>
                            {
                                ForegroundLabel.Text = "Current chunk: " + result + "\n" + "Matched second: " + (3 + FingerprintManager.LatestTimeStamp) + " s" + "\n" + internalMatchCounter + " fingerprints in total." + "\n" + "~ " + (Math.Round(FingerprintManager.LatestTimeStamp / 60)) + " minutes.";
                            });
                        }
                    });
                };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                SetButtonAvailability(true, true, true, true, false);
                RecordManager.StopRecord();
                ForegroundLabel.Text = "Stopped recording.";
                _hashedFingerprints = null;
                _selectedMovie = null;
                FingerprintManager.LatestTimeStamp = 0;
            };
            
            // Event handler for simple "Get fingerprints" button click and release.
            GetFingerprintsButton.TouchUpInside += async (sender, e) =>
            {
                var client = new HttpClient();
                var inputString =
                    $"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle='{_selectedMovie}'";

                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                _receivedHashes = responseString.Split(';');


                var inputString2 =
                    $"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='{_selectedMovie}'";
                client.BaseAddress = new Uri(inputString2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response2 = await client.GetAsync(client.BaseAddress);
                var responseString2 = response2.Content.ReadAsStringAsync().Result;
                _receivedTimestamps = responseString2.Split(';');

                FingerprintManager manager = new FingerprintManager();
                var movie = manager.GenerateHashedFingerprints(_receivedHashes, _receivedTimestamps);
                RecordManager.SetHashedFingerprints(movie);
                RecordManager.SetReceivedHashes(_receivedHashes);
                RecordManager.SetReceivedTimestamps(_receivedTimestamps);
                ForegroundLabel.Text = "Found " + _receivedHashes.Length + " fingerprints for " + _selectedMovie + ".";
                _hashedFingerprints = manager.SplitFingerprintLists(movie);
                SetButtonAvailability(true, true, true, true, false);
            };

            MoviePicker.TouchUpInside += (sender, e) =>
            {
                var width = View.Bounds.Width;
                var height = View.Bounds.Height;
                _table = new UITableView(new CGRect(0, 0, width, height))
                {
                    AutoresizingMask = UIViewAutoresizing.All,
                    Source = new TableSource(_availableMovies, this)
                };
                Add(_table);
                SetButtonAvailability(true, true, false, false, false);
            };
        }
        /// <summary>
        /// Set the currently selected movie.
        /// </summary>
        /// <param name="inputMovie">The selected movie.</param>
        public void SetSelectedMovie(string inputMovie)
        {
            _selectedMovie = inputMovie;
        }

        /// <summary>
        /// Set the foreground label from the background.
        /// </summary>
        /// <param name="text">Label text</param>
        public void SetForegroundLabel(string text)
        {
            ForegroundLabel.Text = text;
        }

        /// <summary>
        /// Get the hashed fingerprints fetched from the Web API.
        /// </summary>
        /// <returns></returns>
        public static List<HashedFingerprint[]> GetHashedFingerprints()
        {
            return _hashedFingerprints;
        }

        /// <summary>
        /// Set button states to enabled or disabled.
        /// </summary>
        public void SetButtonAvailability(bool moviePicker, bool getFingerprints, bool longRecord, bool shortRecord, bool stop)
        {
            MoviePicker.Enabled = moviePicker;
            GetFingerprintsButton.Enabled = getFingerprints;
            LongRecordButton.Enabled = longRecord;
            RecordButton.Enabled = shortRecord;
            StopButton.Enabled = stop;
        }
    }
}