using System;
using System.Threading;
using NAudio.Wave;

namespace mt_commonlib
// TODO: Receive .mp4 file (or possibly other formats) from dbapp.
// TODO: Execute fingerprinting algorithm (Algorithm.cs)
// TODO: Send hash directly to back-end

{
    class Fingerprint : IFingerprint
    {
        static void Main(string[] test)
        {
            playFile(@"C:\Users\Kristoffer\Desktop\mt-project\MetaTracks\mt_commonlib\Resources\testFile.mp4");
            Console.WriteLine(@"Application ended . . .");
            Console.ReadKey();
        }

        public void ReceiveMovie(Object file)
        {
            throw new NotImplementedException();
        }

        public void Receive(object file)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFingerprint(Object file)
        {
            throw new NotImplementedException();
        }

        public void Send(string hash)
        {
            throw new NotImplementedException();
        }

        public void Preprocess(object file)
        {
            throw new NotImplementedException();
        }

        public void ComputeSpectrogram(object file)
        {
            throw new NotImplementedException();
        }

        public void Filter(Array stringArray)
        {
            throw new NotImplementedException();
        }

        public void ComputeWavelets(Array stringArray)
        {
            throw new NotImplementedException();
        }

        public void HashTransform(Array waveletArray)
        {
            throw new NotImplementedException();
        }

        public void Send()
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

        public static void playFile(String file)
        {
            using (var reader = new MediaFoundationReader(file))
            using (var waveout = new WaveOutEvent())
            {
                waveout.Init(reader);
                waveout.Play();

                while (waveout.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }

            }
        }
    }
}
