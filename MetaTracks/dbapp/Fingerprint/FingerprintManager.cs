using System;
using System.Diagnostics;
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

            new Thread(() =>
            {
                Console.WriteLine(@"Loaded file: " + filepath);

                const int desiredFrequency = 5512; // 5512 contains all the relevant information
                const int desiredChannels = 1; // Mono
                var testVideo = new Video(filepath);
                Mp4ToWav(testVideo, filepath + "wavOutput.wav");
                var convertedVideo = new Video(filepath + "wavOutput.wav");
                // Video converted to .wav from input multimedia
                Preprocess(convertedVideo, filepath + "preprocessedOutput.wav", desiredFrequency, desiredChannels);
                var preprocessedVideo = new Video(filepath + "preprocessedOutput.wav");
                // Preprocessed .wav, reduced to 5512Hz mono

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
            }).Start();
        }

#region Split wave file
        // Random counter to add to filenames
        private static int _counter;
        /// <summary>
        /// Splits a wave file into many parts, the size of each split is determined by ms given
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="outPath"></param>
        /// <param name="ms"></param>
        public static void SplitWavFile(string inPath, int ms)
        {
            var wavDir = Path.GetDirectoryName(inPath);
            var outPath = Path.Combine(wavDir, Path.GetFileNameWithoutExtension(inPath));
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                // Total frames over the whole file
                var totalFrames = reader.Length;
                // Total frames over 1000 milliseconds
                var framesPerSecond = (long)(totalFrames / reader.TotalTime.TotalMilliseconds)*ms;
                
                long i = 0;
                while(_counter < (reader.TotalTime.TotalMilliseconds)/ms)
                {
                    i++;
                    _counter++;
                    // Creates file named filename__counter[x].wav
                    using (NAudioCode.WaveFileWriter writer = new NAudioCode.WaveFileWriter(outPath +"_" + _counter + ".wav", reader.WaveFormat))
                    {
                        // Start position is i and end position is the next increment
                        // If sentence just as a safekeeping measure so we dont run into unexpected errors
                        if((i += framesPerSecond) <= totalFrames)
                            SplitWavFile(reader, writer, reader.Position, reader.Position+framesPerSecond);
                    }
                }
            }
        }

        private static void SplitWavFile(WaveFileReader reader, NAudioCode.WaveFileWriter writer, long startPos, long endPos)
        {
            reader.Position = startPos;
            // Creates a new buffer with 1024 bytes
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                
                // Bytes still left to read
                int bytesRequired = (int) (endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    // Bytes to read next, picks the smallest value of bytesRequired or buffer.length
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);

                    // Reades bytes from buffer into variable
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        // Writes bytes from buffer into file
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
#endregion
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

        public void DisposeWave()
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

        public void SendToTable(string hash)
        {
            
        }
    }
}