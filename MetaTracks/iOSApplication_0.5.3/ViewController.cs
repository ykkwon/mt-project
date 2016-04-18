using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class ViewController : UIViewController
    {
        public static int SAMPLE_RATE;
        public static int CHANNELS;
        public static AudioFormatType AUDIO_FORMAT;
        public static AVAudioQuality AUDIO_QUALITY;

        string _selectedMovie;
        UITableView _table;
        public static List<HashedFingerprint[]> useThis = null;
        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            SAMPLE_RATE = 5512;
            CHANNELS = 1;
            AUDIO_FORMAT = AudioFormatType.LinearPCM;
            AUDIO_QUALITY = AVAudioQuality.Max;

            base.ViewDidLoad();
            RecordManager.Observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(RecordManager.OnDidPlayToEndTime);
            string[] availableMovies = null;
            string[] receivedHashes;
            string[] receivedTimestamps;
            double counter = 0;



            GetFingerprintsButton.Enabled = false;
            RecordButton.Enabled = false;
            TestButton.Enabled = false;
            MoviePicker.Enabled = false;
            StopButton.Enabled = false;
            IndexButton.Enabled = true;

            IndexButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            GetFingerprintsButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            RecordButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            StopButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            MoviePicker.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            TestButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);

            // short record
            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {
                TestButton.Enabled = false;
                RecordButton.Enabled = false;
                StopButton.Enabled = true;
                IndexButton.Enabled = false;
                MoviePicker.Enabled = false;
                GetFingerprintsButton.Enabled = false;
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        var session = AVAudioSession.SharedInstance();
                        NSError error;
                        session.SetCategory(AVAudioSession.CategoryRecord, out error);
                        RecordManager.CreateOutputUrl(i);
                        RecordManager.PrepareAudioRecording(i, SAMPLE_RATE, AUDIO_FORMAT, CHANNELS, AUDIO_QUALITY);
                        RecordManager.Recorder.Record();
                        Thread.Sleep(3000);
                        RecordManager.Recorder.Stop();
                        var kasdf = RecordManager.AudioFilePath;
                        var test = RecordManager.ConsumeWaveFileShort(kasdf.RelativePath);
                        counter += test;

                        var counter1 = counter;
                        InvokeOnMainThread(() =>
                        {
                            ForegroundLabel.Text = "Matched second: " + (3 + FingerprintManager.LatestTimeStamp) + " s" +
                                                   "\n" + counter1 + " fingerprints in total." + "\n" + "~ " +
                                                   (Math.Round(FingerprintManager.LatestTimeStamp/60)) + " minutes.";
                        });
                    }
                });
            };


            // Long record
            TestButton.TouchUpInside += (sender, e) =>
            {
                    IndexButton.Enabled = false;
                    RecordButton.Enabled = false;
                    StopButton.Enabled = true;
                    IndexButton.Enabled = false;
                    GetFingerprintsButton.Enabled = false;
                    MoviePicker.Enabled = false;

                    var preSession = AVAudioSession.SharedInstance();
                    NSError preError;
                    preSession.SetCategory(AVAudioSession.CategoryRecord, out preError);
                    RecordManager.CreateOutputUrl(0);
                    RecordManager.PrepareAudioRecording(0, SAMPLE_RATE, AUDIO_FORMAT, CHANNELS, AUDIO_QUALITY);
                    RecordManager.Recorder.Record();
                    Thread.Sleep(10000);
                    RecordManager.Recorder.Stop();
                    var prePath = RecordManager.AudioFilePath;
                    var result = RecordManager.ConsumeFirstFile(prePath.RelativePath, useThis);

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
                                RecordManager.ConsumeFirstFile(prePath.RelativePath, useThis);
                            }
                            var session = AVAudioSession.SharedInstance();
                            NSError error;
                            session.SetCategory(AVAudioSession.CategoryRecord, out error);
                            RecordManager.CreateOutputUrl(i);
                            RecordManager.PrepareAudioRecording(i, SAMPLE_RATE, AUDIO_FORMAT, CHANNELS, AUDIO_QUALITY);
                            RecordManager.Recorder.Record();
                            Thread.Sleep(3000);
                            RecordManager.Recorder.Stop();
                            var kasdf = RecordManager.AudioFilePath;
                            var test = RecordManager.ConsumeWaveFile(kasdf.RelativePath, result);
                            counter += test;

                            var counter1 = counter;
                            InvokeOnMainThread(() =>
                            {

                                ForegroundLabel.Text = "Current chunk: " + result + "\n" + "Matched second: " + (3 + FingerprintManager.LatestTimeStamp) + " s" + "\n" + counter1 + " fingerprints in total." + "\n" + "~ " + (Math.Round(FingerprintManager.LatestTimeStamp / 60)) + " minutes.";
                            });
                        }
                    });
                };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                RecordManager.StopRecord();
                TestButton.Enabled = true;
                RecordButton.Enabled = true;
                MoviePicker.Enabled = true;
                ForegroundLabel.Text = "Stopped recording.";
                useThis = null;
                _selectedMovie = null;
                FingerprintManager.LatestTimeStamp = 0;
                IndexButton.Enabled = true;
                StopButton.Enabled = false;
                GetFingerprintsButton.Enabled = true;
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
                receivedHashes = responseString.Split(';');


                var inputString2 =
                    $"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='{_selectedMovie}'";
                client.BaseAddress = new Uri(inputString2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response2 = await client.GetAsync(client.BaseAddress);
                var responseString2 = response2.Content.ReadAsStringAsync().Result;
                receivedTimestamps = responseString2.Split(';');

                FingerprintManager manager = new FingerprintManager();
                var movie = manager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps);
                RecordManager.SetHashedFingerprints(movie);
                RecordManager.SetReceivedHashes(receivedHashes);
                RecordManager.SetReceivedTimestamps(receivedTimestamps);
                ForegroundLabel.Text = "Found " + receivedHashes.Length + " fingerprints for " + _selectedMovie + ".";
                RecordButton.Enabled = true;
                TestButton.Enabled = true;
                useThis = manager.SplitFingerprintLists(movie);
            };

            // Event handler for simple "Index movies" button click and release.
            IndexButton.TouchUpInside += async (sender, e) =>
            {
                var client = new HttpClient();
                var inputString =
                @"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL";
                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                availableMovies = responseString.Split(',');
                Array.Sort(availableMovies);
                ForegroundLabel.Text = "Indexing done. Found " + (availableMovies.Length - 1) + " movies in the database.";
                MoviePicker.Enabled = true;
            };

            MoviePicker.TouchUpInside += (sender, e) =>
            {
                var width = View.Bounds.Width;
                var height = View.Bounds.Height;
                _table = new UITableView(new CGRect(0, 0, width, height))
                {
                    AutoresizingMask = UIViewAutoresizing.All,
                    Source = new TableSource(availableMovies, this)
                };
                Add(_table);
                GetFingerprintsButton.Enabled = true;
                TestButton.Enabled = false;
                RecordButton.Enabled = false;
                StopButton.Enabled = false;
            };
        }
        public void SetSelectedMovie(string inputMovie)
        {
            _selectedMovie = inputMovie;
        }

        public void SetForegroundLabel(string text)
        {
            ForegroundLabel.Text = text;
        }
    }
}