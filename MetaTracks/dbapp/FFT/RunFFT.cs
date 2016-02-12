using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;

namespace dbApp.FFT
{
    class RunFFT
    {
        public event EventHandler<FftEventArgs> FftCalculated;
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        public RunFFT(string filepath)
        {
            try
            {
                var inputStream = new AudioFileReader(filepath);
                var aggregator = new SampleAggregator(inputStream)
                {
                    NotificationCount = inputStream.WaveFormat.SampleRate/100,
                    PerformFFT = true
                };

                aggregator.FftCalculated += (sender, args) => OnFftCalculated(args);
                aggregator.MaximumCalculated += (sender, args) => OnMaximumCalculated(args);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem has occured in fftrun");
            }
        }

        protected virtual void OnFftCalculated(FftEventArgs e)
        {
            EventHandler<FftEventArgs> handler = FftCalculated;
            handler?.Invoke(this, e);

            //Console.WriteLine("fileStream Position: " + fileStream.CurrentTime);
        }
        
        protected virtual void OnMaximumCalculated(MaxSampleEventArgs e)
        {
            EventHandler<MaxSampleEventArgs> handler = MaximumCalculated;
            handler?.Invoke(this, e);
        }
    }
}
