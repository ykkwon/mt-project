using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Threading;
using AVFoundation;
using Foundation;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using System.Collections.Generic;
using AcousticFingerprintingLibrary.SoundFingerprint;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies.Strides;

namespace iOSApplication
{   
    public static class Record
    {
        static AVAudioRecorder _recorder;
        static NSError _error;
        static NSUrl _url;
        static NSDictionary _settings;
        private static ArrayList currentRecordedFiles;
        //static Fingerprinter fp = new Fingerprinter();


        public static void StopRecording()
        {
            _recorder.Stop();
        }

        public static string PrepareRecording(int fileIterator)
        {
            string fileName = string.Format("Myfile{0}.wav", fileIterator);
            string audioFilePath = Path.Combine(Path.GetTempPath(), fileName);

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
            return audioFilePath;
        }

        public static void DoRecord()
        {
            // This writes data to the wave file.
            _recorder.Record();
            Thread.Sleep(3000);
            _recorder.Stop();
        }

        public static void RunRecord()
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                var thisFile = PrepareRecording(i);
                DoRecord();
                ConsumeWaveFile(thisFile);
            }

        }

        
        public static void ConsumeWaveFile(string filePath)
        {
            bassInitMethod();
            float[] monoArray = ReadMonoFromFile(filePath, 5512, 0, 0);
            Fingerprinter manager = new Fingerprinter();
            int strideSize = 1102;
            int samplesPerFingerprint = 128*64;
            IStride stride = new IncrementalStaticStride(1102, 128*64);
            var fingerprints = manager.CreateFingerprints(monoArray, stride);
            Console.WriteLine("Fingerprints: " + fingerprints[0].ToString());
        }

        public static void bassInitMethod()
        {
            //Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
            BassNet.Registration("gleb.godonoga@gmail.com", "2X155323152222");
            //Dummy calls made for loading the assemblies
            //int bassVersion = Bass.BASS_GetVersion();
            int bassMixVersion = BassMix.BASS_Mixer_GetVersion();
            //int bassfxVersion = BassFx.BASS_FX_GetVersion();
            //int plg = Bass.BASS_PluginLoad("bassflac.dll");
            //if (plg == 0)throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            //if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero)) //Set Sample Rate / MONO
            //    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            //if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_MIXER_FILTER, 50)) /*Set filter for anti aliasing*/
            //    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            //if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true)) /*Set floating parameters to be passed*/
            //    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        public static float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmillisecond)
        {
            int totalmilliseconds = milliseconds <= 0 ? Int32.MaxValue : milliseconds + startmillisecond;
            float[] data = null;
            //create streams for re-sampling
            int stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); //Decode the stream
            if (stream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            int mixerStream = BassMix.BASS_Mixer_StreamCreate(samplerate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT))//BASSFlag.BASS_MIXER_FILTER))
            {
                int bufferSize = samplerate * 20 * 4; /*read 10 seconds at each iteration*/
                float[] buffer = new float[bufferSize];
                List<float[]> chunks = new List<float[]>();
                int size = 0;
                while ((float)(size) / samplerate * 1000 < totalmilliseconds)
                {
                    //get re-sampled/mono data
                    int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, bufferSize);
                    if (bytesRead == 0)
                        break;
                    float[] chunk = new float[bytesRead / 4]; //each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesRead / 4);
                    chunks.Add(chunk);
                    size += bytesRead / 4; //size of the data
                }

                if ((float)(size) / samplerate * 1000 < (milliseconds + startmillisecond))
                    return null; /*not enough samples to return the requested data*/
                int start = (int)((float)startmillisecond * samplerate / 1000);
                int end = (milliseconds <= 0) ? size : (int)((float)(startmillisecond + milliseconds) * samplerate / 1000);
                data = new float[size];
                int index = 0;
                /*Concatenate*/
                foreach (float[] chunk in chunks)
                {
                    Array.Copy(chunk, 0, data, index, chunk.Length);
                    index += chunk.Length;
                }
                /*Select specific part of the song*/
                if (start != 0 || end != size)
                {
                    float[] temp = new float[end - start];
                    Array.Copy(data, start, temp, 0, end - start);
                    data = temp;
                }
            }
            else
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(stream);
            return data;
        }
    }
}

