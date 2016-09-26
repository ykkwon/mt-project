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
using static iOSApplication_0._5._3.RecordManager;

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
        private string[] _mediaTypes;
        private string[] _receivedHashes;
        private string[] _receivedTimestamps;
        private double _matchCounter;


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

            var titleResponse = await client.GetAsync(client.BaseAddress);
            var responseString = titleResponse.Content.ReadAsStringAsync().Result;
            _availableMovies = responseString.Split(',');

            const string mediaTypeQuery = @"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllMediaTypesSQL";
            client.BaseAddress = new Uri(mediaTypeQuery);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var mediaTypeResponse = await client.GetAsync(client.BaseAddress);
            var mediaTypeString = mediaTypeResponse.Content.ReadAsStringAsync().Result;
            _mediaTypes = mediaTypeString.Split(',');


            MoviePicker.Enabled = true;
            ForegroundLabel.Text = "Indexing done.";
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
            Observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnDidPlayToEndTime);

            _sampleRate = 5512;
            _channels = 1;
            _audioFormat = AudioFormatType.LinearPCM;
            _audioQuality = AVAudioQuality.Max;

            base.ViewDidLoad();
            ForegroundLabel.Text = "Movies are being indexed . . .";
            GetFingerprintsButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            RecordButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            StopButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            MoviePicker.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            SetButtonAvailability(false, false, false, false);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {
                SetButtonAvailability(false, false, false, true);
                // Is short, i.e trailer (6.2 min or less) and can be sequentially iterated as a whole.
                if (_hashedFingerprints.Count <= 2)
                {
                    var session = AVAudioSession.SharedInstance();
                    NSError error;
                    session.SetCategory(AVAudioSession.CategoryRecord, out error);

                    Task.Run(() =>
                    {

                        for (var i = 1; i <= 1000; i++) // TODO: Replace this.
                        {
                            InitializeComponents();
                            CreateOutputUrl(i);
                            Console.WriteLine(i);
                            PrepareAudioRecording(i, _sampleRate, _audioFormat, _channels, _audioQuality);
                            Recorder.Record();
                            Thread.Sleep(3000); // TODO: Improve this. 
                            Recorder.Stop();
                            var currentWaveFile = AudioFilePath;
                            var consumedWaveFileShort = RecordShortFirst(currentWaveFile.RelativePath);
                            _matchCounter += consumedWaveFileShort;

                            var internalMatchCounter = _matchCounter;
                            InvokeOnMainThread(() =>
                            {
                                ForegroundLabel.Text = "Matched second: " + (3 + FingerprintManager.LatestTimeStamp) +
                                                       " s" +
                                                       "\n" + internalMatchCounter + " fingerprints in total." + "\n" +
                                                       "~ " +
                                                       (Math.Round(FingerprintManager.LatestTimeStamp/60)) + " minutes.";
                            });
                        }
                    });
                }

                // Is long (6.2 min or more), should be split.
                else if (_hashedFingerprints.Count > 2)
                {
                    var session = AVAudioSession.SharedInstance();
                    NSError error;
                    session.SetCategory(AVAudioSession.CategoryRecord, out error);

                    Task.Run(() =>
                    {

                        for (var i = 1; i <= 1000; i++) // TODO: Replace this.
                        {
                            InitializeComponents();
                            CreateOutputUrl(i);
                            Console.WriteLine(i);
                            PrepareAudioRecording(i, _sampleRate, _audioFormat, _channels, _audioQuality);
                            Recorder.Record();
                            Thread.Sleep(3000); // TODO: Improve this. 
                            Recorder.Stop();
                            var currentWaveFile = AudioFilePath;
                            var consumedWaveFileShort = RecordShortFirst(currentWaveFile.RelativePath);
                            _matchCounter += consumedWaveFileShort;

                            var internalMatchCounter = _matchCounter;
                            InvokeOnMainThread(() =>
                            {
                                ForegroundLabel.Text = "Matched second: " + (3 + FingerprintManager.LatestTimeStamp) +
                                                       " s" +
                                                       "\n" + internalMatchCounter + " fingerprints in total." + "\n" +
                                                       "~ " +
                                                       (Math.Round(FingerprintManager.LatestTimeStamp/60)) + " minutes.";
                            });
                        }
                    });
                }
            };

                // Event handler for simple "Stop" button click and release.
                StopButton.TouchUpInside += (sender, e) =>
            {
                SetButtonAvailability(true, true, true, false);
                StopRecord();
                ForegroundLabel.Text = "Stopped recording.";
                _hashedFingerprints = null;
                _selectedMovie = null;
                FingerprintManager.LatestTimeStamp = 0;
            };
            
            // Event handler for simple "Get fingerprints" button click and release.
            GetFingerprintsButton.TouchUpInside += async (sender, e) =>
            {
                ForegroundLabel.Text = "Getting fingerprints for " + _selectedMovie + " . . . \n" + "This might take a while, depending on the media length.";
                var client = new HttpClient();
                var inputString =
                    $"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle='{_selectedMovie}'";
                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                _receivedHashes = responseString.Split(';');


                var inputString2 =
                    $"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='{_selectedMovie}'";
                client.BaseAddress = new Uri(inputString2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                var response2 = await client.GetAsync(client.BaseAddress);
                var responseString2 = response2.Content.ReadAsStringAsync().Result;
                _receivedTimestamps = responseString2.Split(';');

                var manager = new FingerprintManager();
                var movie = manager.GenerateHashedFingerprints(_receivedHashes, _receivedTimestamps);
                SetHashedFingerprints(movie);
                SetReceivedHashes(_receivedHashes);
                SetReceivedTimestamps(_receivedTimestamps);
                ForegroundLabel.Text = "Found " + _receivedHashes.Length + " fingerprints for " + _selectedMovie + ".";
                _hashedFingerprints = manager.SplitFingerprintLists(movie);
                SetButtonAvailability(true, true, true, false);
            };

            MoviePicker.TouchUpInside += (sender, e) =>
            {
                var width = View.Bounds.Width;
                var height = View.Bounds.Height;
                _table = new UITableView(new CGRect(0, 0, width, height))
                {
                    AutoresizingMask = UIViewAutoresizing.All,
                    Source = new TableSource(_availableMovies, _mediaTypes, this)
                };
                Add(_table);
                SetButtonAvailability(true, true, false, false);
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
        public void SetButtonAvailability(bool moviePicker, bool getFingerprints, bool shortRecord, bool stop)
        {
            MoviePicker.Enabled = moviePicker;
            GetFingerprintsButton.Enabled = getFingerprints;
            RecordButton.Enabled = shortRecord;
            StopButton.Enabled = stop;
        }
    }
}