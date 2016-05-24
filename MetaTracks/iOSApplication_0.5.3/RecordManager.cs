using System.Collections.Generic;
using System.IO;
using AcousticFingerprintingLibrary_0._4._5;
using AVFoundation;
using Foundation;
using AudioToolbox;

namespace iOSApplication_0._5._3
{
    /// <summary>
    ///  Class responsible for recording, observing and processing microphone input. 
    /// </summary>
    public class RecordManager
    {
        internal static AVAudioRecorder Recorder;
        internal static AVPlayer Player;
        internal static NSUrl AudioFilePath;
        internal static NSObject Observer;
        internal static FingerprintManager Manager = new FingerprintManager();
        internal static string[] ReceivedHashes;
        internal static string[] ReceivedTimestamps;

        private static readonly List<HashedFingerprint> StoredFingerprints = new List<HashedFingerprint>();
        private static HashedFingerprint[] _movie;
        private static List<HashedFingerprint[]> _hashedFingerprints;
        private static string _tempRecording;
        private static int _secondaryIndex;

        /// <summary>
        /// Creates the disposable Wave file.
        /// </summary>
        /// <param name="nameIterator">Used to iterate the file name.</param>
        /// <returns>Returns the file path in the temporary system folder.</returns>
        public static NSUrl CreateOutputUrl(int nameIterator)
        {
            var fileName = $"split{nameIterator}.wav";
            _tempRecording = Path.Combine(Path.GetTempPath(), fileName);

            return NSUrl.FromFilename(_tempRecording);
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
        public static bool PrepareAudioRecording(int nameIterator, int sampleRate, AudioFormatType format, int channels,
            AVAudioQuality quality)
        {
            AudioFilePath = CreateOutputUrl(nameIterator);

            var audioSettings = new AudioSettings
            {
                SampleRate = 5512,
                Format = AudioFormatType.LinearPCM,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.Max,
            };

            NSError error;
            Recorder = AVAudioRecorder.Create(AudioFilePath, audioSettings, out error);
            if (error != null)
            {
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
        /// TODO: Make the first consume work properly.
        /// </summary>
        /// <param name="filePath">Current wave recording file path</param>
        /// <param name="list">List of all possible sublist chunks</param>
        /// <returns>The index of the most probable playback section.</returns>
        /// 

            /*
        public static int RecordLongFirst(string filePath, List<HashedFingerprint[]> list)
        {
            var monoArray = BassLoader.GetSamplesMono(filePath, 5512);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = Manager.CreateFingerprints(monoArray);

            var test = Manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
            {
                StoredFingerprints.Add(hash);
            }
            _secondaryIndex = Manager.FindBestFingerprintList(list, StoredFingerprints.ToArray());
            return _secondaryIndex;
        }
        */

        /// <summary>
        /// Processes wave files continuously, only used in ViewController's Record (LONG).
        /// </summary>
        /// <param name="filePath">Current wave recording file path</param>
        /// <param name="index">Current index</param>
        /// <returns>Double where -1 is no match, anything else is a probable match.</returns>
        public static double RecordLongSecond(string filePath, int index)
        {
            // Read all the mono values from the input file.
            var monoArray = BassLoader.GetSamplesMono(filePath, 5512);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = Manager.CreateFingerprints(monoArray);

            var test = Manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
            {
                StoredFingerprints.Add(hash);
            }
            var results = Manager.CompareFingerprintListsHighest(_hashedFingerprints[index],
                StoredFingerprints.ToArray());
            return results;
        }

        /// <summary>
        /// Processes wave files continuously, only used in ViewController's Record (SHORT).
        /// </summary>
        /// <param name="filePath">Current wave recording file path</param>
        /// <returns>Double where -1 is no match, anything else is a probable match.</returns>
        public static double RecordShortFirst(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = BassLoader.GetSamplesMono(filePath, 5512);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = Manager.CreateFingerprints(monoArray);

            var test = Manager.GetFingerHashes(preliminaryFingerprints);
            foreach (var hash in test)
                StoredFingerprints.Add(hash);
            var results = Manager.CompareFingerprintListsHighest(_movie, StoredFingerprints.ToArray());
            StoredFingerprints.Clear();
            return results;
        }
    

/// <summary>
        /// Attempts to stop the recording.
        /// TODO: Fix the possible infinite loop error
        /// </summary>
        public static void StopRecord()
        {
            if (Recorder == null) return;
            Recorder.Stop();
            Recorder.Dispose();
            ReceivedHashes = null;
            ReceivedTimestamps = null;
            _movie = null;
            StoredFingerprints.Clear();
        }

        /// <summary>
        /// Initialize the audio recording session.
        /// </summary>
        public static void InitializeComponents()
        {
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Record);
            _hashedFingerprints = ViewController.GetHashedFingerprints();
        }

        /// <summary>
        /// Set the HashedFingerprints array.
        /// </summary>
        /// <param name="fingerprint"></param>
        public static void SetHashedFingerprints(HashedFingerprint[] fingerprint)
        {
            _movie = fingerprint;
        }

        /// <summary>
        /// Get the ReceivedHashes array.
        /// </summary>
        /// <param name="receivedHashes"></param>
        public static void SetReceivedHashes(string[] receivedHashes)
        {
            ReceivedHashes = receivedHashes;
        }

        /// <summary>
        /// Set the ReceivedTimestamps array.
        /// </summary>
        /// <param name="receivedTimestamps"></param>
        public static void SetReceivedTimestamps(string[] receivedTimestamps)
        {
            ReceivedTimestamps = receivedTimestamps;
        }
    }
}
