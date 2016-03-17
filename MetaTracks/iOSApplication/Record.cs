using System;
using System.IO;
using System.Threading;
using AcousticFingerprintingLibrary;
using AcousticFingerprintingLibrary.SoundFingerprint;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies.Strides;
using AVFoundation;
using Foundation;

namespace iOSApplication
{
    public static class Record
    {
        static AVAudioRecorder _recorder;
        static NSError _error;
        static NSUrl _url;
        static NSDictionary _settings;

        public static void StopRecording()
        {
            new Thread(() =>
            {
                if (_recorder.Recording)
                {
                    _recorder.Stop();
                }
                Console.WriteLine("Recorder is not currently running.");
            }).Start();
        }

        public static void PrepareRecording(int fileIterator)
        {
            new Thread(() =>
            {
                var audioSession = AVAudioSession.SharedInstance();
                audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
                string fileName = string.Format("Myfile{0}.wav", fileIterator);
                string audioFilePath = Path.Combine("/Users/metatracks/Desktop", fileName);
                Console.WriteLine("Audio File Path: " + audioFilePath);

                _url = NSUrl.FromFilename(audioFilePath);
                //set up the NSObject Array of values that will be combined with the keys to make the NSDictionary
                NSObject[] values =
                {
                    NSNumber.FromFloat(5512.0f), //Sample Rate
                    NSNumber.FromInt32((int) AudioToolbox.AudioFormatType.LinearPCM), //AVFormat
                    NSNumber.FromInt32(1), //Channels
                    NSNumber.FromInt32(16), //PCMBitDepth
                    NSNumber.FromBoolean(false), //IsBigEndianKey
                    NSNumber.FromBoolean(false) //IsFloatKey 
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

                //Set Settings with the Values and Keys to create the NSDictionary
                _settings = NSDictionary.FromObjectsAndKeys(values, keys);

                //Set recorder parameters
                _recorder = AVAudioRecorder.Create(_url, new AudioSettings(_settings), out _error);
                //Set Recorder to Prepare To Record
                _recorder.PrepareToRecord();
                DoRecord();
            }).Start();
        }

        public static void DoRecord()
        {
            (new Thread(() =>
            {
                {
                    // This writes data to the wave file.
                    _recorder.Record();
                    Thread.Sleep(1000);
                    _recorder.Stop();
                    Console.WriteLine("Finished recording.");
                }
            })).Start();
        }
    }
}
