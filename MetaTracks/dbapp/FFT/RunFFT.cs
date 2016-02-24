using System;
using dbApp.Fingerprint;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace dbApp.FFT
{
    class RunFFT
    {
        private static int fftLength = 8192;
        private static readonly SampleAggregator SampleAggregator = new SampleAggregator(fftLength);
        private static IWaveIn waveIn;

        private static WaveOutEvent _waveOut;
        private static NotifyingSampleProvider _nsp;
        private static FadeInOutSampleProvider _fadeInOut;
        

        public RunFFT(Video vid)
        {
            
        }

        // ReSharper disable once InconsistentNaming
        public static void StartFFT(Video vid)
        {
            /*
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
            // Wavein loopback
            
            waveIn = new WasapiLoopbackCapture();
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.StartRecording();
            */
            //
            
            _waveOut = new WaveOutEvent();
            AudioFileReader audioFile = new AudioFileReader(vid.FilePath);

            _nsp = new NotifyingSampleProvider(audioFile.ToSampleProvider());
            _nsp.Sample += OnDataAvailable;
            var bfp = new BufferedWaveProvider(audioFile.WaveFormat);
            _fadeInOut = new FadeInOutSampleProvider(_nsp);
            _waveOut.Init(bfp);

            //sampleAggregator.NotificationCount = audioFile.WaveFormat.SampleRate/1000;
            SampleAggregator.PerformFFT = true;
            SampleAggregator.FftCalculated += FftCalculated;
        }
        static void OnDataAvailable(object sender, SampleEventArgs e)
        //static void OnDataAvailable(object sender, WaveInEventArgs e)
        {/*
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new EventHandler<SampleEventArgs>(OnDataAvailable), sender, e);
                //Dispatcher.CurrentDispatcher.BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
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
            }*/
            SampleAggregator.Add(e.Left);
        }

        private static void FftCalculated(object sender, FftEventArgs e)
        {
            for (var i = 0; i < e.Result.Length; i++)
            {
                
                //Console.WriteLine("FFT output.");
                Console.WriteLine(e.Result[i].X);
                //Console.WriteLine(e.Result[i].Y);
                
            }
        }

    }
}
