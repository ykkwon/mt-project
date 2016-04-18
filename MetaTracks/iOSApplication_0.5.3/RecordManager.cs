using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AcousticFingerprintingLibrary_0._4._5;
using AcousticFingerprintingLibrary_0._4._5.DistanceClasses;
using AVFoundation;
using Foundation;

namespace iOSApplication_0._5._3
{
    public static class RecordManager
    {
        public static string[] ReceivedHashes;
        public static string[] ReceivedTimestamps;
        public static HashedFingerprint[] Movie;

        public static AVAudioRecorder Recorder;
        public static AVPlayer Player;
        public static Stopwatch Stopwatch;
        public static NSUrl AudioFilePath;
        public static NSObject Observer;
        public static string TempRecording;
        public static int Secondresults;

        public static NSUrl CreateOutputUrl(int nameIterator)
        {
            string fileName = string.Format("Myfile{0}.wav", nameIterator);
            TempRecording = Path.Combine(Path.GetTempPath(), fileName);
            
            return NSUrl.FromFilename(TempRecording);
        }

        public static void OnDidPlayToEndTime(object sender, NSNotificationEventArgs e)
        {
            Player.Dispose();
            Player = null;
        }

        

        public static bool PrepareAudioRecording(int nameIterator)
        {
            AudioFilePath = CreateOutputUrl(nameIterator);

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
            //Console.WriteLine("Done Recording (status: {0})", e.Status);
        }

        public static void Dispose(bool disposing)
        {
            Observer.Dispose();
        }

        private static List<HashedFingerprint> storedFingerprints = new List<HashedFingerprint>();

        public static int ConsumeFirstFile(string filePath, List<HashedFingerprint[]> list)
        {
            
            var monoArray = BassProxy.ReadMonoFromFileStatic(filePath, 5512, 0, 0);
            FingerprintManager manager = new FingerprintManager();
            Distance distance = new IncrementalDistance(1102, 128 * 64);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray);

            var test = manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
                storedFingerprints.Add(hash);
            //                                             // String[], String[], int lshSize
            //var results = manager.GetTimeStamps(movie, storedFingerprints.ToArray());

            Secondresults = manager.FindBestFingerprintList(list, storedFingerprints.ToArray());
            
            return Secondresults;
        }

        public static double ConsumeWaveFile(string filePath, int index)
        {
            // Read all the mono values from the input file.
            var monoArray = BassProxy.ReadMonoFromFileStatic(filePath, 5512, 0, 0);
            FingerprintManager manager = new FingerprintManager();
            Distance distance = new IncrementalDistance(1102, 128 * 64);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray);

            var test = manager.GetFingerHashes(preliminaryFingerprints);
            foreach(var hash in test)
                storedFingerprints.Add(hash);
            //                                             // String[], String[], int lshSize
            //var results = manager.GetTimeStamps(movie, storedFingerprints.ToArray());
            
            var results = manager.CompareFingerprintListsHighest(ViewController.useThis[index], storedFingerprints.ToArray());
            if (results != -1)
            {
                //Console.WriteLine("SecondResult: " + Secondresults + " Index: " + ViewController.useThis.Count);
                if (index >= Secondresults)
                    if (manager.CheckIteration(FingerprintManager.LatestTimeStamp, ViewController.useThis[Secondresults+1]))
                        Secondresults++;
                // If amatch is found, print timestamp
                // Console.WriteLine("Matched -- " + results);
                storedFingerprints.Clear();
                return results;
            }
            Console.WriteLine("NO MATCH -- " + storedFingerprints[0].HashBins[0]);
            storedFingerprints.Clear();

            /*****
            (useThis) var movieList = manager.SplitFingerprintLists(Movie);
            (results) var newIndex = manager.FindBestFingerprintList(movieList, storedFingerprints.ToArray());
            if (movieList.Count > newIndex)
            {
                if (manager.CheckIteration(results, movieList[newIndex + 1]))
                {
                    newIndex++;
                }
            }
            ***********/////////

            return results;
        }

        public static double ConsumeWaveFileShort(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = BassProxy.ReadMonoFromFileStatic(filePath, 5512, 0, 0);
            FingerprintManager manager = new FingerprintManager();
            Distance distance = new IncrementalDistance(1102, 128 * 64);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray);

            var test = manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
                storedFingerprints.Add(hash);
            //                                             // String[], String[], int lshSize
            //var results = manager.GetTimeStamps(movie, storedFingerprints.ToArray());
            var results = manager.CompareFingerprintListsHighest(Movie, storedFingerprints.ToArray());
            if (results != -1)
            {
                // If amatch is found, print timestamp
                // Console.WriteLine("Matched -- " + results);
                storedFingerprints.Clear();
                return results;
            }
            Console.WriteLine("NO MATCH -- " + storedFingerprints[0].HashBins[0]);
            storedFingerprints.Clear();
            return results;
        }

        // TODO: Aware that this will run forever in some cases
        public static void StopRecord()
        {       
            if (Recorder != null)
            {
                Recorder.Stop();
                Recorder.Dispose();
                ReceivedHashes = null;
                ReceivedTimestamps = null;
                Movie = null;
                
                
                storedFingerprints.Clear();
            }
        }

        public static void InitializeComponents()
        {
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
        }

        public static void SetHashedFingerprints(HashedFingerprint[] fingerprint)
        {
            Movie = fingerprint;
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
