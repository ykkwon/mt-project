﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint;
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
            string[] receivedHashes = null;
            string[] receivedTimestamps = null;

            MovieTextField.EnablesReturnKeyAutomatically = true;
            MovieTextField.ReturnKeyType = UIReturnKeyType.Send;


            Thread recordThread = new Thread(RecordManager.RunRecord);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) =>
            {
                recordThread.Start();

                
                try {
                    if (recordThread.ThreadState == ThreadState.Running) {
                        Console.WriteLine("Thread is already running.");
                    }
                    else
                    {
                        RecordManager.InitializeComponents();
                        recordThread.Start();
                        ForegroundLabel.Text = "Recording . .";
                        
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

                var textFieldInput = MovieTextField.Text.ToString();
                Console.WriteLine("User input: " + textFieldInput);
                Console.WriteLine("Sending request to Web API: " + textFieldInput);
                var client = new HttpClient();
                var inputString = string.Format("http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllFingerprintsSQL?inputTitle=" + "'{0}'",
                    textFieldInput);
                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                receivedHashes = responseString.Split(',');
                Console.WriteLine("Got all hashes.");
                

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                var inputString2 = string.Format("http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTimestampsSQL?inputTitle=" + "'{0}'",
                    textFieldInput);
                client.BaseAddress = new Uri(inputString2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response2 = await client.GetAsync(client.BaseAddress);
                var responseString2 = response2.Content.ReadAsStringAsync().Result;
                receivedTimestamps = responseString2.Split(',');

                //
                var hashBins = new long[receivedHashes.Length];
                var timestamps = new double[receivedTimestamps.Length];
                List<HashedFingerprint> receivedFingerprints = new List<HashedFingerprint>();
                var previousTimestamp = -1.0;
                var previousIndex = 0;

                for (int index = 0; index < receivedHashes.Length; index++)
                {
                    if (timestamps[index] != previousTimestamp)
                    {
                        previousIndex = index;
                        if (index >= 20)
                        {
                            var bin = new long[20];
                            for (int i = 0; i < 20; i++)
                            {
                                bin[i] = hashBins[previousIndex + i];
                            }
                            receivedFingerprints.Add(new HashedFingerprint(bin, timestamps[index]));
                        }
                    }
                    hashBins[index] = Convert.ToInt64(receivedHashes[index]);
                    timestamps[index] = Convert.ToDouble(receivedTimestamps[index]);
                    previousTimestamp = timestamps[index];
                }
                //
                Console.WriteLine("Got all timestamps.");

                
                Console.WriteLine("RECEIVED HASHES LENGTH: " + receivedHashes.Length);
                Console.WriteLine("RECEIVED TIMESTAPS LENGTH: " + receivedTimestamps.Length);
                Console.WriteLine("ReceivedHashes[0]: " + receivedHashes[0]);
                Console.WriteLine("ReceivedHashes[1]: " + receivedHashes[1]);
                Console.WriteLine("ReceivedHashes[2]: " + receivedHashes[2]);
                Console.WriteLine("ReceivedTimestamps[0]: " + receivedTimestamps[0]);
                Console.WriteLine("ReceivedTimestamps[0]: " + receivedTimestamps[1]);
                Console.WriteLine("ReceivedTimestamps[0]: " + receivedTimestamps[2]);
                
                ForegroundLabel.Text = "Fingerprints " + receivedHashes.Length + "---" +" Timestamps: " + receivedTimestamps.Length;
            };

            IndexButton.TouchUpInside += async (sender, e) =>
            {
                var client = new HttpClient();
                var inputString =
                "http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetAllTitlesSQL";
                client.BaseAddress = new Uri(inputString);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                var responseString = response.Content.ReadAsStringAsync().Result;
                availableMovies = responseString.Split(',');
                Console.WriteLine("Indexing done. Found " + availableMovies.Length + " movies.");
            };


        }
    }
}