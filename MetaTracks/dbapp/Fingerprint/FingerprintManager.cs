using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using dbApp.Fingerprint;
using dbApp.Fingerprint.NAudioCode;

namespace dbApp
{
    // TODO: Confirm that Mp4toWav actually works.
    // TODO: Confirm that Preprocess actually works.
    // TODO: Thread pooled receive.
    // TODO: Receive.. SOCKETS? 
    // TODO: Possibly GUI with some event handlers.
    // TODO: Plot waveline.

    class FingerprintManager
    {
        private DirectSoundOut output;
        private MediaFoundationReader reader;
        public FingerprintManager(string filepath)
        {
            //const string resourcePath = @"../../Resources/";
            string resourcePath = filepath;

            Console.WriteLine(filepath);
            const int desiredFrequency = 5512; // 5512 contains all the relevant information
            const int desiredChannels = 1; // Mono
            var testVideo = new Video(filepath);
            //var testVideo = new Video(resourcePath + @"testTrailer.mp4");
            CleanAndTrunctuate();
            Mp4ToWav(testVideo, filepath + "wavOutput.wav");
            var convertedVideo = new Video(resourcePath + "wavOutput.wav"); // Video converted to .wav from input multimedia
            Preprocess(convertedVideo, resourcePath + "preprocessedOutput.wav", desiredFrequency, desiredChannels);
            var preprocessedVideo = new Video(resourcePath + "preprocessedOutput.wav"); // Preprocessed .wav, reduced to 5512Hz mono

            reader = new MediaFoundationReader(preprocessedVideo.FilePath);
            /*
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
            }*/
            output = new DirectSoundOut();
            output.Init(reader);
            output.Play();
            
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
                    Fingerprint.NAudioCode.WaveFileWriter.CreateWaveFile(outputFile, resampler);
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
                    Fingerprint.NAudioCode.WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
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

        private void DisposeWave()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
        }
    }
}
