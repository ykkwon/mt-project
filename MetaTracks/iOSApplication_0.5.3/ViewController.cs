using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using UIKit;

namespace iOSApplication_0._5._3
{
    public partial class ViewController : UIViewController
    {
        string[] availableMovies = new string[100];

        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public async void getTitles()
        {
            using (var client = new HttpClient())
            {
                var inputString =
                string.Format("http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL");
                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                string s = responseString;
                availableMovies = s.Split(',');
                Console.WriteLine("Movies indexed from database.");
            }
        }

        public override void ViewDidLoad()
        {
            getTitles();

            base.ViewDidLoad();

            Thread recordThread = new Thread(RecordManager.RunRecord);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) => {
                Console.WriteLine(availableMovies[0]);
                //recordThread.Start();

                /*
                try {
                    if (newThread.ThreadState == ThreadState.Running) {
                        Console.WriteLine("Thread is already running.");
                    }
                    else
                    {
                        Record.InitializeComponents();
                        foreground_label.Text = "Recording . .";
                        newThread.Start();
                    }
                }
                catch(ThreadStateException)
                {
                    Console.WriteLine("The recorder is already running.");
                }
                */
            };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                ForegroundLabel.Text = "Stopped recording.";
                recordThread.Abort();
            };
        }
    }
}