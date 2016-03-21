using System;
using System.IO;
using System.Threading;
using AVFoundation;
using Foundation;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using System.Collections.Generic;
using System.Net;
using AcousticFingerprintingLibrary.SoundFingerprint;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies.Strides;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json;

namespace iOSApplication
{
    public static class Record
    {
        static AVAudioRecorder _recorder;
        static NSError _error;
        static NSUrl _url;
        static NSDictionary _settings;
        private static dynamic _json;

        public static void StopRecording()
        {
            // Stop the recorder on "Stop" button click.
            _recorder.Stop();
        }

        public static string PrepareRecording(int fileIterator)
        {
            // Output file name is incremented by fileIterator for each write.
            string fileName = string.Format("Myfile{0}.wav", fileIterator);
            // Write to temporary native filepath. 
            string audioFilePath = Path.Combine(Path.GetTempPath(), fileName);

            _url = NSUrl.FromFilename(audioFilePath);
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

            //Set Settings with the Values and Keys to create the NSDictionary
            _settings = NSDictionary.FromObjectsAndKeys(values, keys);

            //Set recorder parameters
            _recorder = AVAudioRecorder.Create(_url, new AudioSettings(_settings), out _error);
            //Set Recorder to Prepare To Record
            _recorder.PrepareToRecord();
            // Return the file path of the written file as string.
            return audioFilePath;
        }

        public static void DoRecord()
        {
            // Write data to the wave file.
            _recorder.Record();
            // "Sleep", or keep running, for 3000 milliseconds before stopping and resuming.
            Thread.Sleep(3000);
            _recorder.Stop();
        }

        public static void RunRecord()
        {
            // TODO: int.MaxValue can be reduced.
            for (int i = 0; i < int.MaxValue; i++)
            {
                var thisFile = PrepareRecording(i);
                DoRecord();
                ConsumeWaveFile(thisFile);
                File.Delete(thisFile);
            }

        }


        public static void ConsumeWaveFile(string filePath)
        {
            // Read all the mono values from the input file.
            var monoArray = ReadMonoFromFile(filePath, 5512, 0, 0);
            Fingerprinter manager = new Fingerprinter();
            IStride stride = new IncrementalStaticStride(1102, 128 * 64);
            // Create an array of fingerprints to be hashed.
            var preliminaryFingerprints = manager.CreateFingerprints(monoArray, stride);
            SendToApi(preliminaryFingerprints);
        }

        public static void SendToApi(List<Fingerprint> currentFingerprintList)
        {
            Console.WriteLine("Received " + currentFingerprintList.Count + " items. ");
            foreach (var fp in currentFingerprintList)
            {
                var test = "6406f282c6d15db5b2e31b395a92e4a5";
                var request =
                    WebRequest.Create(String.Format("http://webapi-1.bwjyuhcr5p.eu-west-1.elasticbeanstalk.com/Fingerprints/GetSingleFingerprintByHash?inputHash=" + "{0}", test));
                request.ContentType = "application/json";
                request.Method = "GET";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            var content = reader.ReadToEnd();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Console.Out.WriteLine("Response contained empty body...");
                            }
                            else
                            {
                                _json = JsonConvert.DeserializeObject(content, typeof(object));
                                Console.WriteLine(_json);
                                
                            }

                            Assert.NotNull(content);
                        }
                }
            }
            currentFingerprintList.Clear();
            Console.WriteLine("Clearing list. List count is now: " + currentFingerprintList.Count);
        }

        public static float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmillisecond)
        {
            int totalmilliseconds = milliseconds <= 0 ? Int32.MaxValue : milliseconds + startmillisecond;
            float[] data;
            // Create streams for re-sampling
            int stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); // Decode the stream
            if (stream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            int mixerStream = BassMix.BASS_Mixer_StreamCreate(samplerate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)) // BASSFlag.BASS_MIXER_FILTER))
            {
                int bufferSize = samplerate * 20 * 4; /* read 10 seconds at each iteration */
                float[] buffer = new float[bufferSize];
                List<float[]> chunks = new List<float[]>();
                int size = 0;
                while ((float)(size) / samplerate * 1000 < totalmilliseconds)
                {
                    // get re-sampled / mono data
                    int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, bufferSize);
                    if (bytesRead == 0)
                        break;
                    float[] chunk = new float[bytesRead / 4]; // each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesRead / 4);
                    chunks.Add(chunk);
                    size += bytesRead / 4; // size of the data
                }

                if ((float)(size) / samplerate * 1000 < (milliseconds + startmillisecond))
                    return null; /* not enough samples to return the requested data */
                int start = (int)((float)startmillisecond * samplerate / 1000);
                int end = (milliseconds <= 0) ? size : (int)((float)(startmillisecond + milliseconds) * samplerate / 1000);
                data = new float[size];
                int index = 0;
                /* Concatenate */
                foreach (float[] chunk in chunks)
                {
                    Array.Copy(chunk, 0, data, index, chunk.Length);
                    index += chunk.Length;
                }
                /* Select specific part of the input media */
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

