using System;
using System.Collections.Generic;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

//using Un4seen.Bass.Misc;

namespace AcousticFingerprintingLibrary_0._4._5
{
    /// <summary>
    ///   Bass Proxy for Bass.Net API
    /// </summary>
    /// <remarks>
    ///   This class containts all methods using bass.net. Bass.net is a c# wrapper for the bass.dll DSP framework. It is used
    ///   for audio processing.
    /// </remarks>
    public class BassProxy : IDisposable
    {
        /// <summary>
        ///   Default sample rate used at initialization
        /// </summary>
        private const int DefaultSampleRate = 44100;

        /// <summary>
        ///   Shows whether the proxy is already disposed
        /// </summary>
        private bool _alreadyDisposed;
        
        #region Constructors

        static BassProxy()
        {
            //Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
            BassNet.Registration("kristian.stoylen93@gmail.com", "2X20371028152222");
            // Initialize BassMix
            BassMix.BASS_Mixer_GetVersion();

            // Initialize Bass.ll and set config to floating parameters
            if (!Bass.BASS_Init(-1, DefaultSampleRate, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero)) //Set Sample Rate / MONO
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true)) /*Set floating parameters to be passed*/
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        #endregion

        public static float[] GetSamplesMono(string filename, int samplerate)
        {
            int totalmilliseconds = int.MaxValue;
            float[] samples;
            //create streams for re-sampling
            var bassStream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); //Decode the stream
            if (bassStream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            var mixerStream = BassMix.BASS_Mixer_StreamCreate(samplerate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, bassStream, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT))
            {
                var bufferSize = samplerate * 20 * 4; /*read 10 seconds at each iteration*/
                var buffer = new float[bufferSize];
                var chunks = new List<float[]>();
                var size = 0;
                while ((float)size / samplerate * 1000 < totalmilliseconds)
                {
                    //get re-sampled/mono data
                    var bytesToRead = Bass.BASS_ChannelGetData(mixerStream, buffer, bufferSize);
                    if (bytesToRead == 0)
                        break;

                    var chunk = new float[bytesToRead / 4]; //each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesToRead / 4);
                    chunks.Add(chunk);
                    size += bytesToRead / 4; //size of the data
                }

                samples = new float[size];

                /*Concatenate*/
                var cursor = 0;
                for (var i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    Array.Copy(chunk, 0, samples, cursor, chunk.Length);
                    cursor += chunk.Length;
                }
            }
            else
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            // Free bass from memory
            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(bassStream);
            return samples;
        }

        /// <summary>
        ///   Dispose the unmanaged resource. Free bass.dll.
        /// </summary>
        public void Dispose()
        {
            _alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}