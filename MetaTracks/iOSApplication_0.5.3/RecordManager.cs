using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AcousticFingerprintingLibrary_0._4._5;
using AVFoundation;
using Foundation;
using AudioToolbox;

namespace iOSApplication_0._5._3
{
    /// <summary>
    ///  Class responsible for recording, observing and processing microphone input. 
    /// </summary>
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
        public static int SecondaryIndex;
        private static List<HashedFingerprint> storedFingerprints = new List<HashedFingerprint>();

        /// <summary>
        /// Creates the disposable Wave file.
        /// </summary>
        /// <param name="nameIterator">Used to iterate the file name.</param>
        /// <returns>Returns the file path in the temporary system folder.</returns>
        public static NSUrl CreateOutputUrl(int nameIterator)
        {
            string fileName = string.Format("disp{0}.wav", nameIterator);
            TempRecording = Path.Combine(Path.GetTempPath(), fileName);

            return NSUrl.FromFilename(TempRecording);
        }

        /// <summary>
        /// Observer for playback, disposes player when playback is finished.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public static void OnDidPlayToEndTime(object sender, NSNotificationEventArgs e)
        {
            Player.Dispose();
            Player = null;
        }

        /// <summary>
        /// Prepares the audio recording, setting audio recorder parameters and initializes the recorder.
        /// </summary>
        /// <param name="nameIterator">Used to iterate the file name.</param>
        /// <param name="sampleRate">The desired sampling rate.</param>
        /// <param name="format">The desired output format.</param>
        /// <param name="channels">The desired number of channels. (1: mono, 2: stereo)</param>
        /// <param name="quality">The input quality.</param>
        /// <returns>Returns true if no errors are encountered.</returns>
        public static bool PrepareAudioRecording(int nameIterator, int sampleRate, AudioFormatType format, int channels, AVAudioQuality quality)
        {
            AudioFilePath = CreateOutputUrl(nameIterator);

            var audioSettings = new AudioSettings
            {
                SampleRate = 5512,
                Format = AudioToolbox.AudioFormatType.LinearPCM,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.Max,
            };

            NSError error;
            Recorder = AVAudioRecorder.Create(AudioFilePath, audioSettings, out error);
            if (error != null)
            {
                Console.WriteLine(error);
                return false;
            }

            if (!Recorder.PrepareToRecord())
            {
                Recorder.Dispose();
                Recorder = null;
                return false;
            }

            Recorder.FinishedRecording += OnFinishedRecording;

            return true;
        }

        /// <summary>
        /// Ovserver for recorder, which disposes the recorder when recording is finished.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public static void OnFinishedRecording(object sender, AVStatusEventArgs e)
        {
            Recorder.Dispose();
            Recorder = null;
        }

        /// <summary>
        /// Disposes the observer.
        /// </summary>
        /// <param name="disposing">Returns true while observer is disposing.</param>
        public static void Dispose(bool disposing)
        {
            Observer.Dispose();
        }

        /// <summary>
        /// Processes the first "long" wave file, only used in ViewController's Record (LONG).
        /// TODO: Should be merged with ConsumeWaveFile as different overloads.
        /// </summary>
        /// <param name="filePath">Current wave recording file path</param>
        /// <param name="list">List of all possible sublist chunks</param>
        /// <returns>The index of the most probable playback section.</returns>
        public static int ConsumeFirstFile(string filePath, List<HashedFingerprint[]> list)
        {
            var monoArray = BassProxy.ReadMonoFromFileStatic(filePath, 5512, 0, 0);
            FingerprintManager manager = new FingerprintManager();
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray);

            var test = manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
            {
                storedFingerprints.Add(hash);
            }
            SecondaryIndex = manager.FindBestFingerprintList(list, storedFingerprints.ToArray());
            return SecondaryIndex;
        }

        /// <summary>
        /// Processes wave files continuously, only used in ViewController's Record (LONG).
        /// /// TODO: Should be merged with ConsumeWaveFile as different overloads.
        /// </summary>
        /// <param name="filePath">Current wave recording file path</param>
        /// <param name="list">Current index</param>
        /// <returns>Double where -1 is no match, anything else is a probable match.</returns>
        public static double ConsumeWaveFile(string filePath, int index)
        {
            // Read all the mono values from the input file.
            var monoArray = BassProxy.ReadMonoFromFileStatic(filePath, 5512, 0, 0);
            FingerprintManager manager = new FingerprintManager();
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray);

            var test = manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
                storedFingerprints.Add(hash);

            var results = manager.CompareFingerprintListsHighest(ViewController.useThis[index], storedFingerprints.ToArray());
            if (results != -1)
            {
                if (index >= SecondaryIndex)
                    if (manager.CheckIteration(FingerprintManager.LatestTimeStamp, ViewController.useThis[SecondaryIndex + 1]))
                        SecondaryIndex++;
                storedFingerprints.Clear();
                return results;
            }
            Console.WriteLine("NO MATCH -- " + storedFingerprints[0].HashBins[0]);
            storedFingerprints.Clear();
            return results;
        }

        /// <summary>
        /// Processes wave files continuously, only used in ViewController's Record (SHORT).
        /// /// TODO: Should be merged with ConsumeWaveFile as different overloads.
        /// </summary>
        /// <param name="filePath">Current wave recording file path</param>
        /// <returns>Double where -1 is no match, anything else is a probable match.</returns>
        public static double ConsumeWaveFileShort(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = BassProxy.ReadMonoFromFileStatic(filePath, 5512, 0, 0);
            FingerprintManager manager = new FingerprintManager();
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

        /// <summary>
        /// Attempts to stop the recording.
        /// TODO: Fix the possible infinite loop error
        /// </summary>
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

        /// <summary>
        /// Initialize the audio recording session.
        /// </summary>
        public static void InitializeComponents()
        {
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Record);
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
