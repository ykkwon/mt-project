﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Threading;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides;
using AVFoundation;
using Foundation;
using UIKit;

namespace iOSApplication_0._5._3
{
    public static class RecordManager
    {
        public static string[] ReceivedHashes;
        public static string[] ReceivedTimestamps;


        public static AVAudioRecorder Recorder;
        public static AVPlayer Player;
        public static Stopwatch Stopwatch;
        public static NSUrl AudioFilePath;
        public static NSObject Observer;
        public static string tempRecording;
        public static NSUrl CreateOutputUrl()
        {
            string fileName = string.Format("Myfile{0}.wav", DateTime.Now.ToString("yyyyMMddHHmmss"));
            tempRecording = Path.Combine(Path.GetTempPath(), fileName);
        
            return NSUrl.FromFilename(tempRecording);
        }

        public static void OnDidPlayToEndTime(object sender, NSNotificationEventArgs e)
        {
            Player.Dispose();
            Player = null;
        }

        

        public static bool PrepareAudioRecording()
        {
            AudioFilePath = CreateOutputUrl();

            var audioSettings = new AudioSettings
            {
                SampleRate = 5512,
                Format = AudioToolbox.AudioFormatType.LinearPCM,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.High
            };

            //Set recorder parameters
            NSError error;
            Recorder = AVAudioRecorder.Create(AudioFilePath, audioSettings, out error);
            if (error != null)
            {
                Console.WriteLine(error);
                return false;
            }

            //Set Recorder to Prepare To Record
            if (!Recorder.PrepareToRecord())
            {
                Recorder.Dispose();
                Recorder = null;
                return false;
            }

            Recorder.FinishedRecording += OnFinishedRecording;

            return true;
        }

        public static void OnFinishedRecording(object sender, AVStatusEventArgs e)
        {
            Recorder.Dispose();
            Recorder = null;
            Console.WriteLine("Done Recording (status: {0})", e.Status);
        }

        public static void Dispose(bool disposing)
        {
            Observer.Dispose();
        }


        /*
        public static void RunRecord()
        {
           {
                for (var i = 0; i < int.MaxValue; i++)
                {
                    var preprocessedFile = PrepareRecording(i);
                    ConsumeWaveFile(preprocessedFile);
                }
           }
        }*/


        private static List<HashedFingerprint> storedFingerprints = new List<HashedFingerprint>(); 
        public static double ConsumeWaveFile(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = BassProxy.ReadMonoFromFile(filePath, 5512, 0, 0);
            Fingerprinter manager = new Fingerprinter();
            IStride stride = new IncrementalStaticStride(1102, 128 * 64);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray, stride);

            var test = manager.GetFingerHashes(stride, preliminaryFingerprints);
            foreach(var hash in test)
                storedFingerprints.Add(hash);
            
            var movie = GenerateHashedFingerprints(ReceivedHashes, ReceivedTimestamps);
            //var results = manager.GetTimeStamps(movie, storedFingerprints.ToArray());
            int x = 0;
            var results = manager.CompareFingerprintListsHighest(movie, storedFingerprints.ToArray());
            if (results != -1)
            {
                // If amatch is found, print timestamp
                Console.WriteLine("Matched -- " + results);
                //storedFingerprints.Clear();
                return results;
            }
            else
            {
                Console.WriteLine("NO MATCH -- " + storedFingerprints[0].HashBins[0]);
                storedFingerprints.Clear();
                return results;
            }
        }

        public static HashedFingerprint[] GenerateHashedFingerprints(string[] receivedHashes, string[] receivedTimestamps)
        {
            List<long> hashBins = new List<long>();
            List<double> timestamps = new List<double>();

            List<HashedFingerprint> receivedFingerprints = new List<HashedFingerprint>();
            try {
                for (int index = 0; index < receivedHashes.Length - 1; index++)
                {
                    hashBins.Add(Convert.ToInt64(receivedHashes[index]));
                    timestamps.Add(Convert.ToDouble(receivedTimestamps[index]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            List<long[]> hashBinsList = new List<long[]>();
            List<double> timestampList = new List<double>();
            var indexer = 0;
            for (int j = 0; j < timestamps.Count - 1; j++)
            {
                if (j % 20 == 0 && hashBins.Count > j + 20)
                {
                    long[] bins = new long[20];
                    for (int i = 0; i < 20; i++)
                    {
                        bins[i] = hashBins[i + j];
                    }
                    hashBinsList.Add(bins);
                    timestampList.Add(timestamps[j]);
                    indexer += bins.Length;
                }
            }
            for (int i = 0; i < hashBinsList.Count; i++)
            {
                var finger = new HashedFingerprint(hashBinsList[i], timestampList[i]);
                receivedFingerprints.Add(finger);

            }

            return receivedFingerprints.ToArray();
        }

        public static void StopRecord()
        {
            Recorder.Stop();
            Recorder.Dispose();
        }

        public static void InitializeComponents()
        {
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
        }

        public static void SetReceivedHashes(string[] receivedHashes)
        {
            ReceivedHashes = receivedHashes;
        }

        public static void SetReceivedTimestamps(string[] receivedTimestamps)
        {
            ReceivedTimestamps = receivedTimestamps;
        }
    }
}
