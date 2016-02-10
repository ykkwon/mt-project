using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace mt_commonlib
{
    // TODO: Confirm that Mp4toWav actually works.
    // TODO: Confirm that Preprocess actually works.
    // TODO: Thread pooled receive.
    // TODO: Receive.. SOCKETS? 
    // TODO: Possibly GUI with some event handlers.
    // TODO: Plot waveline.

    class FingerprintManager
    {
        static void Main()
        {
            const string resourcePath = @"..\..\Resources\";
            const int desiredFrequency = 5512; // 5512 contains all the relevant information
            const int desiredChannels = 1; // Mono
            var fpm = new FingerprintManager();
            var testVideo = new Video(resourcePath + "testTrailer.mp4");
            fpm.CleanAndTrunctuate();
            fpm.Mp4ToWav(testVideo, resourcePath + "wavOutput.wav");
            var convertedVideo = new Video(resourcePath + "wavOutput.wav"); // Video converted to .wav from input multimedia
            fpm.Preprocess(convertedVideo, resourcePath + "preprocessedOutput.wav", desiredFrequency, desiredChannels); 
            var preprocessedVideo = new Video(resourcePath + "preprocessedOutput.wav"); // Preprocessed .wav, reduced to 5512Hz mono

            using (var reader = new MediaFoundationReader(preprocessedVideo.FilePath))
            using (var waveout = new WaveOutEvent())
            {
                waveout.Init(reader);
                Console.WriteLine(@"Initiating playback.");
                waveout.Play();


                while (waveout.PlaybackState == PlaybackState.Playing)
                {
                    Console.WriteLine(@"Output: " + waveout.OutputWaveFormat + @" Position: " + waveout.GetPosition() + @" Bytes written to waveout.");
                    Thread.Sleep(5000);
                }
                Console.WriteLine(@"Playback ended.");
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

        public void Preprocess(Video video, string outputFile, int desiredFrequency, int desiredChannels)
        {
            using (var reader = new WaveFileReader(video.FilePath))
            {
                var outFormat = new WaveFormat(desiredFrequency, desiredChannels);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    resampler.ResamplerQuality = 60;
                    WaveFileWriter.CreateWaveFile(outputFile, resampler);
                    Console.WriteLine(@"Preprocessing done.");
                }
            }
        }

        public
            void ComputeSpectrogram()
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

        public void Mp4ToWav(Video video, string outputFile)
        {
            using (MediaFoundationReader reader = new MediaFoundationReader(video.FilePath))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                    Console.WriteLine(@"MP4 to WAV conversion done.");
                }
            }
        }

        public void CleanAndTrunctuate()
        {
            if (File.Exists(@"..\..\Resources\wavOutput.wav"))
            {
                try
                {
                    File.Delete(@"..\..\Resources\wavOutput.wav");
                    Console.WriteLine(@"Old wav output was trunctuated.");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (File.Exists(@"..\..\Resources\preprocessedOutput.wav"))
            {
                try
                {
                    File.Delete(@"..\..\Resources\preprocessedOutput.wav");
                    Console.WriteLine(@"Old preprocessed output was trunctuated.");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
