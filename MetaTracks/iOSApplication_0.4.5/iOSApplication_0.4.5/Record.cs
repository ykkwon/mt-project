using AVFoundation;
using System;
using System.IO;
using System.Threading;
using Foundation;
using Un4seen.Bass;

namespace iOSApplication_0._4._5
{
    public static class Record
    {
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

        }

        public static void SendToWebApi()
        {
            
        }

        public static void StopRecord()
        {
            Recorder.Stop();
            Recorder.Dispose();
        }

        public static void InitializeComponents()
        {
            // Register Bass.NET license
            BassNet.Registration("kristian.stoylen93@gmail.com", "2X20371028152222");
            // Initialize BASS 
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            // Create a new native iOS audio session
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
        }
    }
}