using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using AVFoundation;
using Foundation;
using UIKit;

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
            // Directory<long[], double> fingerprints;
            RecordManager.Observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(RecordManager.OnDidPlayToEndTime);
            string[] availableMovies = new string[100];
            string[] receivedHashes;
            string[] receivedTimestamps;

            MovieTextField.EnablesReturnKeyAutomatically = true;
            MovieTextField.ReturnKeyType = UIReturnKeyType.Send;

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {
                Console.WriteLine("Begin Recording");

                var session = AVAudioSession.SharedInstance();

                NSError error = null;
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

                if (!RecordManager.PrepareAudioRecording())
                {
                    ForegroundLabel.Text = "Error preparing";
                    return;
                }

                if (!RecordManager.Recorder.Record())
                {
                    ForegroundLabel.Text = "Error preparing";
                    return;
                }

                RecordManager.Stopwatch = new Stopwatch();
                RecordManager.Stopwatch.Start();

                ForegroundLabel.Text = "Recording";
                RecordButton.Enabled = false;
                StopButton.Enabled = true;
                PlayButton.Enabled = false;
            };



            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                RecordManager.Recorder.Stop();
                RecordManager.Stopwatch.Stop();

                ForegroundLabel.Text = string.Format("{0:hh\\:mm\\:ss}", RecordManager.Stopwatch.Elapsed);
                ForegroundLabel.Text = "";
                RecordButton.Enabled = true;
                StopButton.Enabled = false;
                PlayButton.Enabled = true;
            };

            SendButton.TouchUpInside += (sender, e) =>
            {
                try
                {
                    var test = RecordManager.ConsumeWaveFile(RecordManager.tempRecording);
                    Console.WriteLine(test);
                }
                catch (Exception ex)
                {
                    ForegroundLabel.Text = "ERROR";
                    Console.WriteLine("There was a problem sending the audio: ");
                    Console.WriteLine(ex.Message);
                }
            };

            PlayButton.TouchUpInside += (sender, e) =>
            {
                try
                {
                    Console.WriteLine("Playing Back Recording {0}", RecordManager.AudioFilePath);

                    // The following line prevents the audio from stopping
                    // when the device autolocks. will also make sure that it plays, even
                    // if the device is in mute
                    NSError error = null;
                    AVAudioSession.SharedInstance().SetCategory(AVAudioSession.CategoryPlayback, out error);
                    if (error != null)
                        throw new Exception(error.DebugDescription);

                    RecordManager.Player = new AVPlayer(RecordManager.AudioFilePath);
                    RecordManager.Player.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem playing back audio: ");
                    Console.WriteLine(ex.Message);
                }
            };

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
                receivedHashes = responseString.Split(',');


                var inputString2 = @"http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle='TH'";
                client.BaseAddress = new Uri(inputString2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response2 = await client.GetAsync(client.BaseAddress);
                var responseString2 = response2.Content.ReadAsStringAsync().Result;
                receivedTimestamps = responseString2.Split(',');

                RecordManager.SetReceivedHashes(receivedHashes);
                RecordManager.SetReceivedTimestamps(receivedTimestamps);
                RecordManager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps);
                ForegroundLabel.Text = "Found " + receivedHashes.Length + " fingerprints for " + "The Hobbit";
            };

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
                ForegroundLabel.Text = "Indexing done. Found " + availableMovies.Length + " movies.";
            };
        }
    }
}