﻿using System;
using System.Collections.Generic;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;

//using Un4seen.Bass.Misc;

namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies
{
    /// <summary>
    ///   Bass Proxy for Bass.Net API
    /// </summary>
    /// <remarks>
    ///   BASS is an audio library for use in Windows and Mac OSX software. 
    ///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons), 
    ///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. 
    ///   All in a tiny DLL, under 100KB* in size.
    /// </remarks>
    public class BassProxy : IAudio
    {
        /// <summary>
        ///   Default sample rate used at initialization
        /// </summary>
        private const int DEFAULT_SAMPLE_RATE = 44100;

        /// <summary>
        ///   Shows whether the proxy is already disposed
        /// </summary>
        private bool _alreadyDisposed;

        /// <summary>
        ///   Currently playing stream
        /// </summary>
        private int _playingStream;

        #region Constructors

        static BassProxy()
        {
            //Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
            //BassNet.Registration("gleb.godonoga@gmail.com", "2X155323152222");

            BassNet.Registration("kristian.stoylen93@gmail.com", "2X20371028152222");
            //Dummy calls made for loading the assemblies
            //int bassVersion = Bass.BASS_GetVersion();
            int bassMixVersion = BassMix.BASS_Mixer_GetVersion();
            //int bassfxVersion = BassFx.BASS_FX_GetVersion();
            //int plg = Bass.BASS_PluginLoad("bassflac.dll");
            //if (plg == 0)throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            if (!Bass.BASS_Init(-1, DEFAULT_SAMPLE_RATE, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero)) //Set Sample Rate / MONO
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true)) /*Set floating parameters to be passed*/
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        /// <summary>
        ///   Public Constructor
        /// </summary>
        public BassProxy()
        {

        }

        #endregion

        #region IAudio Members

        /// <summary>
        ///   Dispose the unmanaged resource. Free bass.dll.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            _alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        float[] IAudio.ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmilliseconds)
        {
            return ReadMonoFromFile(filename, samplerate, milliseconds, startmilliseconds);
        }

        /// <summary>
        ///   Read mono from file
        /// </summary>
        /// <param name = "filename">Name of the file</param>
        /// <param name = "samplerate">Output sample rate</param>
        /// <param name = "milliseconds">Milliseconds to read</param>
        /// <param name = "startmillisecond">Start millisecond</param>
        /// <returns>Array of samples</returns>
        /// <remarks>
        ///   Seeking capabilities of Bass where not used because of the possible
        ///   timing errors on different formats.
        /// </remarks>
        public static float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmillisecond)
        {
            int totalmilliseconds;
            if (milliseconds <= 0) totalmilliseconds = Int32.MaxValue;
            else totalmilliseconds = milliseconds + startmillisecond;
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

        #endregion

        /// <summary>
        ///   Read data from file
        /// </summary>
        /// <param name = "filename">Filename to be read</param>
        /// <param name = "samplerate">Sample rate at which to perform reading</param>
        /// <returns>Array with data</returns>
        public float[] ReadMonoFromFile(string filename, int samplerate)
        {
            return ReadMonoFromFile(filename, samplerate, 0, 0);
        }

        /// <summary>
        ///   Dispose the resources
        /// </summary>
        /// <param name = "isDisposing">If value is disposing</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_alreadyDisposed)
            {
                if (!isDisposing)
                {
                    //release managed resources
                }
                // Bass.BASS_Free();
            }
        }

        /// <summary>
        ///   Finalizer
        /// </summary>
        ~BassProxy()
        {
            Dispose(true);
        }
    }
}