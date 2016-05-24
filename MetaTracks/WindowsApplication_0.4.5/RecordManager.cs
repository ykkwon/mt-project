using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AcousticFingerprintingLibrary_0._4._5;
using NAudio.Wave;

namespace WindowsApplication_0._4._5
{
    public static class RecordManager
    {
        public static string[] ReceivedHashes;
        public static string[] ReceivedTimestamps;
        public static HashedFingerprint[] Movie;

        public static WaveIn Recorder;
        public static WaveOut Player;
        public static WaveFileWriter waveFile;
        public static Stopwatch Stopwatch;
        public static string AudioFilePath;
        public static string TempRecording;


        public static string CreateOutputUrl(int nameIterator)
        {
            string fileName = string.Format("Myfile{0}.wav", nameIterator);
            TempRecording = Path.Combine(Path.GetTempPath(), fileName);
        
            return Path.GetFileName(TempRecording);
        }

        public static void OnDidPlayToEndTime(object sender, EventArgs e)
        {
            Player.Dispose();
            Player = null;
        }

        

        public static bool PrepareAudioRecording(int nameIterator)
        {
            AudioFilePath = CreateOutputUrl(nameIterator);

            Recorder = new WaveIn();
            Recorder.WaveFormat = new WaveFormat(44100, 1);
            waveFile = new WaveFileWriter(AudioFilePath, Recorder.WaveFormat);
            return true;
        }

        public static void OnFinishedRecording(object sender, EventArgs e)
        {
            Recorder.Dispose();
            Recorder = null;
            //Console.WriteLine("Done Recording (status: {0})", e.Status);
        }
        

        private static List<HashedFingerprint> storedFingerprints = new List<HashedFingerprint>();

        public static double ConsumeWaveFile(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = BassLoader.GetSamplesMono(filePath, 5512);
            FingerprintManager manager = new FingerprintManager();
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray);

            var test = manager.GetFingerHashes(preliminaryFingerprints);
            foreach(var hash in test)
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

        public static void StopRecord()
        {
            Recorder.StopRecording();
            Recorder.Dispose();
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
