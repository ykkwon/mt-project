using System;
using System.IO;
using System.Threading;
using System.Windows;
using NAudio.Wave;
using dbApp.Fingerprint;
using NAudio.CoreAudioApi.Interfaces;

namespace dbApp
{
    // TODO: Confirm that Mp4toWav actually works.
    // TODO: Confirm that Preprocess actually works.
    // TODO: Thread pooled receive.
    // TODO: Possibly GUI with some event handlers.
    // TODO: Plot waveline.
    // TODO: Thread the MainWindow!

    class FingerprintManager
    {
        private DirectSoundOut _output;
        private MediaFoundationReader _reader;

        public FingerprintManager(string filepath)
        {
            Console.WriteLine(@"Loaded file: " + filepath);
            const int desiredFrequency = 5512; // 5512 contains all the relevant information
            const int desiredChannels = 1; // Mono
            var testVideo = new Video(filepath);
            CleanAndTrunctuate(); // Remove old output files
            Mp4ToWav(testVideo, filepath + "wavOutput.wav");
            var convertedVideo = new Video(filepath + "wavOutput.wav"); // Video converted to .wav from input multimedia
            Preprocess(convertedVideo, filepath + "preprocessedOutput.wav", desiredFrequency, desiredChannels);
            var preprocessedVideo = new Video(filepath + "preprocessedOutput.wav"); // Preprocessed .wav, reduced to 5512Hz mono
            
            _reader = new MediaFoundationReader(preprocessedVideo.FilePath);
            _output = new DirectSoundOut();
            _output.Init(_reader);
            _output.Play();
            while (_output.PlaybackState == PlaybackState.Playing)
            {
                string currentTime = _reader.CurrentTime.ToString("mm\\:ss"); // TODO: Milliseconds
                // Write every 1000 ms
                Console.WriteLine(currentTime);
                Thread.Sleep(1000);  
            }
            Console.WriteLine(@"Playback has ended.");
            DisposeWave();
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

        public void SliceToFrames()
        {
            
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

        public void Mp4ToWav(Video video, string outputFile)
        {
            using (MediaFoundationReader reader = new MediaFoundationReader(video.FilePath))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    string duration = reader.TotalTime.ToString("mm\\:ss");
                    Console.WriteLine("Playback duration: " + duration);
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
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing) _output.Stop();
                _output.Dispose();
                _output = null;
            }
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }
    }
}