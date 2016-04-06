using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using AVFoundation;
using Foundation;
using UIKit;
using System.Threading;

namespace iOSApplication_0._5._3
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            RecordManager.Observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(RecordManager.OnDidPlayToEndTime);
            string[] availableMovies;
            string[] receivedHashes;
            string[] receivedTimestamps;
            double counter = 0;

            MovieTextField.EnablesReturnKeyAutomatically = true;
            MovieTextField.ReturnKeyType = UIReturnKeyType.Send;

            GetFingerprintsButton.Enabled = false;
            RecordButton.Enabled = false;
            StopButton.Enabled = false;
            PlayButton.Enabled = false;
            SendButton.Enabled = false;
            GetFingerprintsButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            RecordButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            StopButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            PlayButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            SendButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);


            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {
                Console.WriteLine("Begin Recording");

                var session = AVAudioSession.SharedInstance();

                NSError error;
                session.SetCategory(AVAudioSession.CategoryRecord, out error);
                if (error != null)
                {
                    Console.WriteLine(error);
                    return;
                }

                session.SetActive(true, out error);
                if (error != null)
                {
                    Console.WriteLine(error);
                    return;
                }

                if (!RecordManager.PrepareAudioRecording(1))
                {
                    ForegroundLabel.Text = "Error preparing.";
                    return;
                }

                if (!RecordManager.Recorder.Record())
                {
                    ForegroundLabel.Text = "Error preparing.";
                    return;
                }

                RecordManager.Stopwatch = new Stopwatch();
                RecordManager.Stopwatch.Start();

                ForegroundLabel.Text = "Recording . . .";
                RecordButton.Enabled = false;
                StopButton.Enabled = true;
                PlayButton.Enabled = false;
                IndexButton.Enabled = false;
                GetFingerprintsButton.Enabled = false;

                RecordButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
                PlayButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
                IndexButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
                GetFingerprintsButton.SetTitleColor(UIColor.FromRGBA(0, 0, 0, 150), UIControlState.Disabled);
            };


            TestButton.TouchUpInside += (sender, e) =>
            {
                while (true) {
                    for (int i = 0; i < 10; i++)
                    {
                        var session = AVAudioSession.SharedInstance();
                        NSError error;
                        session.SetCategory(AVAudioSession.CategoryRecord, out error);
                        RecordManager.CreateOutputUrl(i);
                        RecordManager.PrepareAudioRecording(i);
                        RecordManager.Recorder.Record();
                        Thread.Sleep(3000);
                        RecordManager.Recorder.Stop();
                        var kasdf = RecordManager.AudioFilePath;
                        Console.WriteLine("TEST: " + kasdf);
                        RecordManager.ConsumeWaveFile(kasdf.RelativePath);
                        Console.WriteLine(RecordManager.AudioFilePath + " consume done");
                        var test = RecordManager.ConsumeWaveFile(RecordManager.TempRecording);
                        counter += test;
                        Console.WriteLine("COUNTER: " + counter);
                        ForegroundLabel.Text = "Matched " + counter + " fingerprints in total.";
                    }
                }
                // Record i 3-4s
                // Skriv til ei fil
                // Send til Preprocess wav
                // Få et resultat
                // Repeatx  
            };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                ForegroundLabel.Text = "Stopping the recorder.";
                RecordManager.Recorder.Stop();
                RecordManager.Stopwatch.Stop();

                ForegroundLabel.Text = $"{RecordManager.Stopwatch.Elapsed:hh\\:mm\\:ss}";
                ForegroundLabel.Text = "";
                RecordButton.Enabled = true;
                StopButton.Enabled = false;
                PlayButton.Enabled = true;
                SendButton.Enabled = true;
               
            };

            SendButton.TouchUpInside += (sender, e) =>
            {
                
                try
                {
                    ForegroundLabel.Text = "Sending recording to database . . .";
                    var test = RecordManager.ConsumeWaveFile(RecordManager.TempRecording);
                    ForegroundLabel.Text = "Matched " + test + " fingerprints in total.";
                }
                catch (Exception ex)
                {
                    ForegroundLabel.Text = "ERROR";
                    Console.WriteLine("There was a problem sending the audio: ");
                    Console.WriteLine(ex.Message);
                }
            };

            // Event handler for simple "Play" button click and release.
            PlayButton.TouchUpInside += (sender, e) =>
            {
                try
                {
                    Console.WriteLine("Playing Back Recording {0}", RecordManager.AudioFilePath);
                    ForegroundLabel.Text = "Playing back recording until end.";

                    // The following line prevents the audio from stopping
                    // when the device autolocks. will also make sure that it plays, even
                    // if the device is in mute
                    NSError error;
                    AVAudioSession.SharedInstance().SetCategory(AVAudioSession.CategoryPlayback, out error);
                    if (error != null)
                        throw new Exception(error.DebugDescription);

                    RecordManager.Player = new AVPlayer(RecordManager.AudioFilePath);
                    RecordManager.Player.Play();
                    ForegroundLabel.Text = "Finished playback.";

                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem playing back audio: ");
                    Console.WriteLine(ex.Message);
                }
            };

            // Event handler for simple "Get fingerprints" button click and release.
            GetFingerprintsButton.TouchUpInside += async (sender, e) =>
            {

                var client = new HttpClient();
                var inputString = @"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle='TH'";

                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                receivedHashes = responseString.Split(';');


                var inputString2 = @"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='TH'";
                client.BaseAddress = new Uri(inputString2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response2 = await client.GetAsync(client.BaseAddress);
                var responseString2 = response2.Content.ReadAsStringAsync().Result;
                receivedTimestamps = responseString2.Split(';');

                RecordManager.SetReceivedHashes(receivedHashes);
                RecordManager.SetReceivedTimestamps(receivedTimestamps);
                ForegroundLabel.Text = "Found " + receivedHashes.Length + " fingerprints for " + "The Hobbit.";
                RecordButton.Enabled = true;
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
                availableMovies = responseString.Split(';');
                ForegroundLabel.Text = "Indexing done. Found " + availableMovies.Length + " movies in the database.";
                GetFingerprintsButton.Enabled = true;
            };
        }
    }
}