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
        public RunFFT(string filepath)
        {
            try
            {

                var inputStream = new AudioFileReader(filepath);
                var aggregator = new SampleAggregator(inputStream);

                aggregator.NotificationCount = inputStream.WaveFormat.SampleRate/100;
                aggregator.PerformFFT = true;
                aggregator.FftCalculated += (sender, args) => OnFftCalculated(args);
                aggregator.MaximumCalculated += (sender, args) => OnMaximumCalculated(args);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem has occured in fftrun");
            }
        }

        private void OnMaximumCalculated(MaxSampleEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnFftCalculated(FftEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
