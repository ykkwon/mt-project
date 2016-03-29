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
                NSNumber.FromFloat(5512.0f), // Sample rate
                NSNumber.FromInt32((int) AudioToolbox.AudioFormatType.LinearPCM), // AVFormat
                NSNumber.FromInt32(1), // Channels
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
            Recorder.PrepareToRecord();
            // Return the file path of the written file as string.
            return audioFilePath;
        }

        public static void DoRecord()
        {
            // Write data to the wave file.
            Recorder.Record();
            // "Sleep", or keep running, for 3000 milliseconds before stopping and resuming.
            Thread.Sleep(3000);
            Recorder.Stop();
        }

        public static void RunRecord()
        {
            {
                for (var i = 0; i < int.MaxValue; i++)
                {
                    var preprocessedFile = PrepareRecording(i);
                    DoRecord();
                    ConsumeWaveFile(preprocessedFile);
                }
            }
        }

        public static void ConsumeWaveFile(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = BassProxy.ReadMonoFromFile(filePath, 5512, 0, 0);
            Fingerprinter manager = new Fingerprinter();
            IStride stride = new IncrementalStaticStride(1102, 128 * 64);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray, stride);

            var test = manager.GetFingerHashes(stride, preliminaryFingerprints);

            foreach (var fingerprint in test)
            {
                Console.WriteLine("HASH BIN: " + fingerprint.HashBins[0] + " --- TIMESTAMP:" + fingerprint.Timestamp);
            }
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
