using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NAudio.MediaFoundation;
using NAudio.Wave;

namespace mt_commonlib
// TODO: Receive .mp4 file (or possibly other formats) from dbapp.
// TODO: Execute fingerprinting algorithm (Algorithm.cs)
// TODO: Send hash directly to back-end

{
    class Fingerprint : IFingerprint
    {
        private static Movie testMovie = new Movie(@"..\..\Resources\testFile.mp4");
        private static String waString; // Debugging 

        static void Main(string[] args)
        {
            PlayFile(testMovie);
            Console.WriteLine(@"Application ended . . .");
            Console.ReadKey();
            //Preprocess(testMovie);
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

        public void Receive(string file)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFingerprint(string file)
        {
            throw new NotImplementedException();
        }

        public void Send(string hash)
        {
            throw new NotImplementedException();
        }

        void IFingerprint.Preprocess(Movie movie)
        {
            Preprocess(movie);
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

        void IFingerprint.PlayFile(Movie movie)
        {
            PlayFile(movie);
        }

        private static void PlayFile(Movie movie)
        {
            {
                using (var reader = new MediaFoundationReader(movie.Path))
                using (var waveout = new WaveOutEvent())
                {
                    waveout.Init(reader);
                    waString = waveout.OutputWaveFormat.ToString();
                    Preprocess(movie);
                    waveout.Play();
                    
                    while (waveout.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }

                }
            }
        }

        public void Send()
        {
            throw new NotImplementedException();
        }

        private static void Preprocess(Movie movie)
        {
            Console.WriteLine(waString);
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

        public void Receive(Movie movie)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFingerprint(Movie movie)
        {
            throw new NotImplementedException();
        }

        public void ComputeSpectrogram(Movie movie)
        {
            throw new NotImplementedException();
        }
    }
}
