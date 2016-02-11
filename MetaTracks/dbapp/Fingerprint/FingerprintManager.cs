using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using System.Threading;

namespace dbApp.Fingerprint
{
    // TODO: Thread pooled receive.
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
                var currentTime = _reader.CurrentTime.ToString("mm\\:ss");
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
                    NAudioCode.WaveFileWriter.CreateWaveFile(outputFile, resampler);
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
                    NAudioCode.WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                    Console.WriteLine(@"MP4 to WAV conversion done.");
                    var duration = reader.TotalTime.ToString("mm\\:ss");
                    Console.WriteLine(@"Playback duration: " + duration);
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