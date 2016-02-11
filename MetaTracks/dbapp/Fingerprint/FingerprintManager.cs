using System;
using System.Diagnostics;
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
            
            // Splits Wave File
            var wavDir = Path.GetDirectoryName(filepath);
            var splitDir = Path.Combine(wavDir, Path.GetFileNameWithoutExtension(filepath));
            SplitWavFile(filepath, splitDir);
            //

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
        }

        // Random counter to add to filenames
        private static int _counter;
        public static void SplitWavFile(string inPath, string outPath)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                // Total frames over the whole file
                var totalFrames = reader.Length;
                // Total frames over 1000 milliseconds
                var framesPerSecond = (long)(totalFrames / reader.TotalTime.TotalMilliseconds)*1000;
                
                long i = 0;
                while(_counter < (reader.TotalTime.TotalMilliseconds)/1000)
                {
                    i++;
                    _counter++;
                    // Creates file named filename__counter[x].wav
                    using (Fingerprint.NAudioCode.WaveFileWriter writer = new Fingerprint.NAudioCode.WaveFileWriter(outPath +"_" + _counter + ".wav", reader.WaveFormat))
                    {
                        // Start position is i and end position is the next increment
                        // If sentence just as a safekeeping measure so we dont run into unexpected errors
                        if((i += framesPerSecond) <= totalFrames)
                            SplitWavFile(reader, writer, reader.Position, reader.Position+framesPerSecond);
                    }
                }
            }
        }

        private static void SplitWavFile(WaveFileReader reader, Fingerprint.NAudioCode.WaveFileWriter writer, long startPos, long endPos)
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

        public void SendToTable(string hash)
        {
            
        }
    }
}