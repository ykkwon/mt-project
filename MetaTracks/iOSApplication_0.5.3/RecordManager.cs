using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides;
using AVFoundation;
using Foundation;

namespace iOSApplication_0._5._3
{
    public static class RecordManager
    {
        public static string[] ReceivedHashes;
        public static string[] ReceivedTimestamps;

        public static AVAudioPlayer Player;
        public static AVAudioRecorder Recorder;
        public static NSError Error;
        public static NSUrl Url;
        public static NSDictionary Settings;

        public static string PrepareRecording(int iterator)
        {
            string fileName = string.Format("Split{0}.wav", iterator);
            string audioFilePath = Path.Combine(Path.GetTempPath(), fileName);
            Url = NSUrl.FromFilename(audioFilePath);

            //set up the NSObject Array of values that will be combined with the keys to make the NSDictionary.
            NSObject[] values =
            {
                // Record in 5512 Hz, Linear PCM, Mono, 16 bit. 
                NSNumber.FromFloat(44100.0f),//5512.0f), // Sample rate
                NSNumber.FromInt32((int) AudioToolbox.AudioFormatType.LinearPCM), // AVFormat
                NSNumber.FromInt32(2), // Channels
                NSNumber.FromInt32(16), // PCM bit depth
                NSNumber.FromBoolean(false), // IsBigEndianKey
                NSNumber.FromBoolean(false) // IsFloatKey 
            };

            //Set up the NSObject Array of keys that will be combined with the values to make the NSDictionary
            NSObject[] keys =
            {
                AVAudioSettings.AVSampleRateKey,
                AVAudioSettings.AVFormatIDKey,
                AVAudioSettings.AVNumberOfChannelsKey,
                AVAudioSettings.AVLinearPCMBitDepthKey,
                AVAudioSettings.AVLinearPCMIsBigEndianKey,
                AVAudioSettings.AVLinearPCMIsFloatKey
            };

            Settings = NSDictionary.FromObjectsAndKeys(values, keys);
            Recorder = AVAudioRecorder.Create(Url, new AudioSettings(Settings), out Error);
            Recorder.Record();
            Thread.Sleep(3000);
            Recorder.Stop();
            Console.WriteLine(audioFilePath);
            Player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(Path.Combine(Path.GetTempPath(), fileName)));


            // Return the file path of the written file as string.
            Console.WriteLine("Returning: " + audioFilePath);
            return audioFilePath;
        }

        public static void RunRecord()
        {
           // {
             //   for (var i = 0; i < int.MaxValue; i++)
            //    {
                    var preprocessedFile = PrepareRecording(1);
            //        ConsumeWaveFile(preprocessedFile);
            //    }
           // }
        }


        private static List<HashedFingerprint> storedFingerprints = new List<HashedFingerprint>(); 
        public static void ConsumeWaveFile(string filePath)
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
            var results = manager.GetTimeStamps(movie, storedFingerprints.ToArray());
            //var results = manager.CompareFingerprintListsHighest(movie, storedFingerprints.ToArray());
            if (results != -1)
            {
                // If amatch is found, print timestamp
                Console.WriteLine("Matched -- " + results);
                //storedFingerprints.Clear();
            }
            else
            {
                //
                Console.WriteLine("NO MATCH -- " + storedFingerprints[0].HashBins[0]);
                storedFingerprints.Clear();
                
            }
            //

            foreach (var fingerprint in test)
            {
                //Console.WriteLine("HASH BIN: " + fingerprint.HashBins[0] + " --- TIMESTAMP:" + fingerprint.Timestamp);
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
            Player.Play();
            //Recorder.Stop();
            //Recorder.Dispose();
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
