using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using dbApp.Fingerprint;
using NAudio.Wave;

namespace dbApp.FFT
{
    class RunFFT
    {
        private static int fftLength = 8192;
        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);
        private IWaveIn waveIn;


        public RunFFT(Media vid)
        {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
            waveIn = new WasapiLoopbackCapture();
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.StartRecording();
            Console.WriteLine("RunFFT Constructor " + vid.FilePath);
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            Console.WriteLine("OnDataAvailable -- RunFFT.cs");
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            }
            else
            {
                byte[] buffer = e.Buffer;
                int bytesRecored = e.BytesRecorded;
                int bufferIncrement = waveIn.WaveFormat.BlockAlign;

                for (int i = 0; i < bytesRecored; i += bufferIncrement)
                {
                    float sample32 = BitConverter.ToSingle(buffer, i);
                    sampleAggregator.Add(sample32);
                }
            }
        }

        void FftCalculated(object sender, FftEventArgs e)
        {
            Console.WriteLine("FFTCalculated");
            for (var i = 0; i < e.Result.Length; i++)
            {
                Console.WriteLine("FFT output.");
                Console.WriteLine(e.Result[i].X);
                Console.WriteLine(e.Result[i].Y);
            }
        }

    }
}
