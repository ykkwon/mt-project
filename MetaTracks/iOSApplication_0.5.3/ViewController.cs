using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
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

            string[] availableMovies = new string[100];
            string[] receivedHashes;
            string[] receivedTimestamps;

            MovieTextField.EnablesReturnKeyAutomatically = true;
            MovieTextField.ReturnKeyType = UIReturnKeyType.Send;


            Thread recordThread = new Thread(RecordManager.RunRecord);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {

                ForegroundLabel.Text = "Recording . .";
                recordThread.Start();
                
                try {
                    if (recordThread.ThreadState == ThreadState.Running) {
                        Console.WriteLine("Thread is already running.");
                    }
                    else
                    {

                        RecordManager.InitializeComponents();
                        recordThread.Start();
                    }
                }
                catch(ThreadStateException)
                {
                    Console.WriteLine("The recorder is already running.");
                }
                
            };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                ForegroundLabel.Text = "Stopped recording.";
                recordThread.Abort();
            };

            GetFingerprintsButton.TouchUpInside += async (sender, e) =>
            {
                //Console.WriteLine("Got input: " + MovieTextField.Text.ToString());
                //var textFieldInput = MovieTextField.Text.ToString();
                //Console.WriteLine("User input: " + textFieldInput);
                //Console.WriteLine("Sending request to Web API: " + textFieldInput);
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

                ForegroundLabel.Text = @"Got all timestamps from database.";


                RecordManager.SetReceivedHashes(receivedHashes);
                RecordManager.SetReceivedTimestamps(receivedTimestamps);
                var hahaha = RecordManager.GenerateHashedFingerprints(receivedHashes, receivedTimestamps);
                ForegroundLabel.Text = @"Got all fingerprints and timestamps.";
                //ForegroundLabel.Text = "Found " + receivedHashes.Length + " fingerprints for " + "The Hobbit";
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