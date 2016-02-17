using System;
using System.Threading;
using NAudio.Wave;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace dbApp.Fingerprint
{
    // TODO: Write relevant information to foreground! 

    class FingerprintManager
    {
        #region CONSTRUCTOR
        public FingerprintManager()
        {
        }
        #endregion

        #region VARIABLES
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private static string tableName = "Video_Fingerprints";
        // 5512 contains all the relevant (perceptive) information
        private static int DesiredFrequency = 5512;
        // One channel is mono as opposed to two which is stereo
        private static int DesiredChannels = 1;
        // Random counter to add to filenames
        private static int _counter;
        #endregion

        #region METHODS
        public static void SplitWavFile(string inPath, string outPath)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                // Total frames over the whole file
                var totalFrames = reader.Length;
                // Total frames over 1000 milliseconds
                var framesPerSecond = (long)(totalFrames / reader.TotalTime.TotalMilliseconds) * 1000;

                long i = 0;
                while (_counter < (reader.TotalTime.TotalMilliseconds) / 1000)
                {
                    i++;
                    _counter++;
                    // Creates file named filename__counter[x].wav
                    using (NAudioCode.WaveFileWriter writer = new NAudioCode.WaveFileWriter(outPath.Remove(outPath.Length - 4) + "_" + _counter + ".wav", reader.WaveFormat))
                    {
                        // Start position is i and end position is the next increment
                        // If sentence just as a safekeeping measure so we dont run into unexpected errors
                        if ((i += framesPerSecond) <= totalFrames)
                            SplitWavFile(reader, writer, reader.Position, reader.Position + framesPerSecond);
                    }
                }
            }
            Console.WriteLine("Splitting done. Split into " + _counter + " chunks.");
            MainWindow.Main.Status = "Splitting done. Split into " + _counter + " chunks.";



        }

        private static void SplitWavFile(WaveFileReader reader, NAudioCode.WaveFileWriter writer, long startPos, long endPos)
        {
            reader.Position = startPos;
            // Creates a new buffer with 1024 bytes
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {

                // Bytes still left to read
                int bytesRequired = (int)(endPos - reader.Position);
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

        public static Video OpenVideo(Video video)
        {
            Console.WriteLine(@"Loaded file: " + video.FilePath);
            MainWindow.Main.Status = "Loaded file: " + video.FilePath;
            Video convertedVideo = Mp4ToWav(video, video.FilePath.Remove(video.FilePath.Length - 4) + "Converted.wav"); // Ugly hack
            Video preprocessedVideo = Preprocess(convertedVideo, convertedVideo.FilePath, DesiredFrequency, DesiredChannels);
            return preprocessedVideo;
        }

        public static void SendToDatabase()
        {
            string[] names = new string[]{ "The Force Awakens", "The Force Awakens", "The Force Awakens", "The Force Awakens"};
            string[] numbers = new string[] { "1", "2", "3", "4" };
            string[] fingerprints = new string[] { "123456", "234567", "345678", "456789" };
           // hash = 1110011001010;

            Table table = Table.LoadTable(client, tableName);
            var input = new Document();
            var number = new Document();
            var fingerprintsInput = new Document();
            int i = 1;
            foreach (string inputs in fingerprints)
            {
                input["Fingerprints"] = inputs;
                input["Timestamp"] = i++;
                input["Name"] = "The Force Awakens";
                table.PutItem(input);
            }


        }


        public static Video Preprocess(Video video, string outputFile, int desiredFrequency, int desiredChannels)
        {
            using (var reader = new WaveFileReader(video.FilePath))
            {
                var outFormat = new WaveFormat(desiredFrequency, desiredChannels);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    resampler.ResamplerQuality = 60;
                    WaveFileWriter.CreateWaveFile(video.FilePath.Remove(video.FilePath.Length - 13) + "Preprocessed.wav", resampler); // Ugly hack
                    Console.WriteLine(@"Preprocessing done.");
                    MainWindow.Main.Status = "Preprocessing done.";
                    var preprocessedVideo = new Video(outputFile);
                    return preprocessedVideo;

                }

            }

        }

        public static void Play(Video video)
        {
            DirectSoundOut _output;
            MediaFoundationReader _reader;
            _reader = new MediaFoundationReader(video.FilePath);
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
            DisposeWave(_output, _reader);
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

        public static Video Mp4ToWav(Video video, string outputFile)
        {
            using (MediaFoundationReader reader = new MediaFoundationReader(video.FilePath))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    NAudioCode.WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                    Console.WriteLine(@"MP4 to WAV conversion done.");
                    MainWindow.Main.Status = "Input has been converted to Wave file.";
                    var convertedVideo = new Video(outputFile);
                    return convertedVideo;
                }
            }
        }

        public static void DisposeWave(DirectSoundOut _output, MediaFoundationReader _reader)
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
        #endregion

    }
}