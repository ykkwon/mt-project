using System;
using System.Threading;
using NAudio.Wave;

namespace mt_commonlib
{
    class FingerprintManager
    {
        public FingerprintManager() { }

        static void Main(string[] args)
        {
            FingerprintManager fpm = new FingerprintManager();
            InputVideo testVideo = new InputVideo(@"..\..\Resources\testFile.mp4");
            fpm.Mp4ToWav(testVideo, "wavOutput.wav");
            using (var reader = new MediaFoundationReader(testVideo.FilePath))
            using (var waveout = new WaveOutEvent())
            {
                waveout.Init(reader);
                waveout.Play();


                while (waveout.PlaybackState == PlaybackState.Playing)
                {
                    Console.WriteLine(waveout.OutputWaveFormat);
                    Thread.Sleep(1000);
                }
            }
        }
        
    

        public void ReceiveMovie()
        {
            throw new NotImplementedException();
        }

        public void ReceiveFingerprint()
        {
            throw new NotImplementedException();
        }

        public void SendToDatabase()
        {
            throw new NotImplementedException();
        }

        public void Preprocess()
        {
            throw new NotImplementedException();
        }


        public void ComputeSpectrogram()
        {
            throw new NotImplementedException();
        }

        public void Filter()
        {
            throw new NotImplementedException();
        }

        public void ComputeWavelets()
        {
            throw new NotImplementedException();
        }

        public void HashTransform()
        {
            throw new NotImplementedException();
        }

        public void Mp4ToWav(InputVideo video, string outputFile)
        {
            using (MediaFoundationReader reader = new MediaFoundationReader(video.FilePath))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                }
            }
        }
    }
}
