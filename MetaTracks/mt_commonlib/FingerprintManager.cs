using System;
using System.Resources;
using System.Threading;
using NAudio.Wave;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SecurityToken;
using Amazon.Runtime;

namespace mt_commonlib
{
    class FingerprintManager
    {

        // Sets up an AmazonDynamoDBClient called client.
        private static AmazonDynamoDBClient client;  
        
        public FingerprintManager() { }

        static void Main(string[] args)
        {
            DynamoDBConfiguration();

        }

        public void NotAnImplementedMethod()
        {
            try
            {
                System.IO.File.Delete(@"..\..\Resources\wavOutput.wav");
                Console.WriteLine(@"Old wav output was trunctuated.");
                System.IO.File.Delete(@"..\..\Resources\preprocessedOutput.wav");
                Console.WriteLine(@"Old preprocessed output was trunctuated.");
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            int desiredFrequency = 5512;
            int desiredChannels = 1; // Mono
            FingerprintManager fpm = new FingerprintManager();
            Video testVideo = new Video(@"..\..\Resources\testTrailer.mp4");
            fpm.Mp4ToWav(testVideo, @"..\..\Resources\wavOutput.wav");
            Video convertedVideo = new Video(@"..\..\Resources\wavOutput.wav");
            fpm.Preprocess(convertedVideo, @"..\..\Resources\preprocessedOutput.wav", desiredFrequency, desiredChannels);
            Video preprocessedVideo = new Video(@"..\..\Resources\preprocessedOutput.wav");

            using (var reader = new MediaFoundationReader(preprocessedVideo.FilePath))
            using (var waveout = new WaveOutEvent())
            {
                waveout.Init(reader);
                Console.WriteLine("Initiating playback.");
                waveout.Play();


                while (waveout.PlaybackState == PlaybackState.Playing)
                {
                    Console.WriteLine("Output: " + waveout.OutputWaveFormat + " Position: " + waveout.GetPosition() + " Bytes written to waveout.");
                    Thread.Sleep(5000);
                }
                Console.WriteLine("Playback ended.");
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

        public static void DynamoDBConfiguration()
        {
            try {
                // Creates a config that can be used by the client.
                AmazonDynamoDBConfig config = new AmazonDynamoDBConfig();
                // the specific dynamoDB URL to find the actual DB data.
                config.ServiceURL = "http://dynamodb.us-west-2.amazonaws.com";
                // Configures the client with credentials and endpoint URL.
                client = new AmazonDynamoDBClient(config);

                SendToDatabase();

                Console.WriteLine("Data sent to DB. To continue, press Enter");
            }
            catch (AmazonDynamoDBException e) { Console.WriteLine("DynamoDB Message:" + e.Message); }
            catch (AmazonServiceException e) { Console.WriteLine("Service Exception:" + e.Message); }
            catch (Exception e) { Console.WriteLine("General Exception:" + e.Message); }
            Console.ReadLine();
        }

        public static void SendToDatabase()
        {
            // hardcoded population of database..
            Table Fingerprints = Table.LoadTable(client, "Fingerprints");

            var d1 = new Document();

            d1["Fingerprint"] = "{&r{Xb#^ZuA}z(gu/C_>gd(nh25#5#S";
            d1["Title"] = "Star Wars: The Force Awakens";
            Fingerprints.PutItem(d1);

            var d2 = new Document();

            d2["Fingerprint"] = "n2R-Jf+N6`.QL,!zKy`f^5Y_U,`U8W`+";
            d2["Title"] = "Deadpool";
            Fingerprints.PutItem(d2);
            //throw new NotImplementedException();
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
                    Console.WriteLine("Preprocessing done.");
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
                    Console.WriteLine("MP4 to WAV conversion done.");
                }
            }
        }
    }
}
