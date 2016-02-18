using System;
using System.Threading;
using NAudio.Wave;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Amazon.DynamoDBv2.Model;

namespace dbApp.Fingerprint
{
    // TODO: BatchWrite to DynamoDB
    // TODO: Add Type to input

    class FingerprintManager
    {
        #region CONSTRUCTOR
        public FingerprintManager() { }
        #endregion

        #region VARIABLES
        private static List<string> splitVideosList = new List<string>();
        private static List<string> hashedChunks = new List<string>();
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
                        {
                            var currString = writer.Filename;
                            splitVideosList.Add(currString);
                            SplitWavFile(reader, writer, reader.Position, reader.Position + framesPerSecond);

                        }
                        
                    }
                }
            }
            Console.WriteLine("Splitting done. Split into " + _counter + " chunks.");
            MainWindow.Main.Status = "Splitting done. Split into " + _counter + " chunks.";
            Console.WriteLine("Initiating hashing");
            HashTransform(splitVideosList);
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

        public static void SendToDatabase(String entryName)
        {
            Console.WriteLine("Sending hashes to database");
            MainWindow.Main.Status = "Sending hashes to database";

            Table table = Table.LoadTable(client, tableName);
            var input = new Document();
            int i = 1;
            foreach (string inputs in hashedChunks)
            {
                input["Fingerprints"] = inputs;
                input["Timestamp"] = i++;
                input["Title"] = entryName;
                input["Type"] = "N/A";
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

        public static void HashTransform(List<string> videoChunks)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                foreach (string item in videoChunks)
                {
                    var hash = GetMd5Hash(md5Hash, item);
                    hashedChunks.Add(hash);
                }
                for (int i = 0; i < hashedChunks.Count; i++)
                {
                    Console.WriteLine(hashedChunks[i]);
                }
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
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

        #region DATABASE OPERATIONS
        public static void CreateTable()
        {

            // Attribute definitions
            var attributeDefinitions = new List<AttributeDefinition>()
            {
                {new AttributeDefinition {AttributeName = "Fingerprint", AttributeType = "S"}},
                {new AttributeDefinition {AttributeName = "Timestamp", AttributeType = "N"}},
                {new AttributeDefinition {AttributeName = "Title", AttributeType = "S"}},
                {new AttributeDefinition {AttributeName = "Type", AttributeType = "S"}}
            };

            // Key schema for table
            var tableKeySchema = new List<KeySchemaElement>() {
                {
                    new KeySchemaElement {
                        AttributeName= "Fingerprint",
                        KeyType = "HASH"  //Partition key
                    }
                },
                {
                    new KeySchemaElement {
                        AttributeName = "Timestamp",
                        KeyType = "RANGE"  //Sort key
                    }
                }
            };

            // Initial provisioned throughput settings for the indexes
            var ptIndex = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5,
            };

            // CreateDateIndex
            var createDateIndex = new GlobalSecondaryIndex()
            {
                IndexName = "FingerprintIndex",
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                    new KeySchemaElement {
                        AttributeName = "Fingerprint", KeyType = "HASH"  //Partition key
                    },
                    new KeySchemaElement {
                        AttributeName = "Timestamp", KeyType = "RANGE"  //Sort key
                    }
                },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };

            // TitleIndex
            var titleIndex = new GlobalSecondaryIndex()
            {
                IndexName = "TitleIndex",
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                    new KeySchemaElement {
                        AttributeName = "Title", KeyType = "HASH"  //Partition key
                    },
                    new KeySchemaElement {
                        AttributeName = "Type", KeyType = "RANGE"  //Sort key
                    }
                },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };

            var TimestampTitleIndex = new GlobalSecondaryIndex()
            {
                IndexName = "Timestamp-Title-index",
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                    new KeySchemaElement {
                        AttributeName = "Timestamp", KeyType = "HASH"  //Partition key
                    },
                    new KeySchemaElement {
                        AttributeName = "Title", KeyType = "RANGE"  //Sort key
                    }
                },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };



            var createTableRequest = new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = (long)1,
                    WriteCapacityUnits = (long)1
                },
                AttributeDefinitions = attributeDefinitions,
                KeySchema = tableKeySchema,
                GlobalSecondaryIndexes = { createDateIndex, titleIndex, TimestampTitleIndex }
            };

            Console.WriteLine("Creating table " + tableName + "...");
            client.CreateTable(createTableRequest);

            WaitUntilTableReady(tableName);
            Console.WriteLine("** Completed creating table **");

        }

        private static void WaitUntilTableReady(string tableName)
        {
            string status = null;
            // Let us wait until table is created. Call DescribeTable.
            do
            {
                System.Threading.Thread.Sleep(5000); // Wait 5 seconds.
                try
                {
                    var res = client.DescribeTable(new DescribeTableRequest
                    {
                        TableName = tableName
                    });

                    Console.WriteLine("Table name: {0}, status: {1}",
                                   res.Table.TableName,
                                   res.Table.TableStatus);
                    status = res.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                    // DescribeTable is eventually consistent. So you might
                    // get resource not found. So we handle the potential exception.
                }
            } while (status != "ACTIVE");
        }

        public static void DeleteTable()
        {
            Console.WriteLine("\n*** Deleting table ***");
            var request = new DeleteTableRequest
            {
                TableName = tableName
            };

            var response = client.DeleteTable(request);

            Console.WriteLine("Table is being deleted...");
        }
        #endregion
    }
}