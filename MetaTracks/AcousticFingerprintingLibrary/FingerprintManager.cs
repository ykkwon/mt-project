using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using NAudio.Wave;

namespace AcousticFingerprintingLibrary
{
    public class FingerprintManager : IFingerprintManager
    {
        public Media ConvertToWav(Media inputMedia)
        {
            // Write to the same folder as the input media
            // TODO: Write to memory, not disk (possibly disk in a temp vol. folder)
            var outputPath = inputMedia.DirPath + "/Converted.wav";
            {
                using (var reader = new MediaFoundationReader(inputMedia.FilePath))
                {
                    using (var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        // Create a new wave file using PCM WaveStream
                        
                        WaveFileWriter.CreateWaveFile(outputPath, pcmStream);
                        var convertedMedia = new Media(outputPath);
                        return convertedMedia;
                    }
                }
            }
        }

        public Media Preprocess(Media inputMedia, int sampleRate)
        {
            {
                // Write to the same folder as the input media
                // TODO: Write to memory, not disk (possibly disk in a temp vol. folder)
                var outPath = inputMedia.DirPath + "/Preprocessed.wav";
                {
                    // read the input media
                    using (var reader = new WaveFileReader(inputMedia.FilePath))
                    {
                        // output format will be written with desired sample rate and as mono
                        var outFormat = new WaveFormat(sampleRate, 1);
                        using (var resampler = new MediaFoundationResampler(reader, outFormat))
                        {
                            // resampler quality ranges from 1 (lowest, linear interpolation) to 60 (highest)
                            resampler.ResamplerQuality = 60;
                            WaveFileWriter.CreateWaveFile(outPath, resampler);
                        }
                        // store the preprocessed video file path
                        var preprocessedVideo = new Media(outPath);
                        return preprocessedVideo;
                    }
                }
            }
        }

        public List<string> SplitWavFile(Media inputMedia, Media outputMedia, int overlapFactor, int splitLength)
        {
            var counter = 0;
            var _prevPos = 0;
            var splitVideosList = new List<string>();
            // TODO: Make this work
            {
                // Save to a new folder called SplitOutput
                // TODO: Write files to memory, not disk.
               string outputFolderName = inputMedia.FilePath + "/../SplitOutput/";
                
                // Create a new directory on disk for output files
                // TODO: WILL NOT WORK FOR A PORTABLE APP, SYSTEM.IO DOES NOT EXIST
                //System.IO.Directory.CreateDirectory(outputFolderName);
                using (WaveFileReader reader = new WaveFileReader(inputMedia.FilePath))
                {
                    // Total frames over the whole file
                    var totalFrames = reader.Length;

                    // Total frames over 1000 milliseconds
                    var framesPerSecond = (long)((totalFrames / reader.TotalTime.TotalMilliseconds) * 1000);

                    while (reader.Position < reader.Length)
                    {
                        counter++;

                        // if reader.position == 0, splitpos = 0 -- else splitpos = Math.Max(0, (prevPos + (framesPerSecond / robustSplit)));
                        var splitPosition = reader.Position == 0 ? 0 : Math.Max(0, (_prevPos + (framesPerSecond / overlapFactor)));

                        reader.Position = splitPosition;

                        // Creates file named filename__counter[x].wav
                        using (WaveFileWriter writer = new WaveFileWriter(outputFolderName + "Split_" + counter + ".wav", reader.WaveFormat))
                        {
                            var currString = writer.Filename;
                            splitVideosList.Add(currString);
                            // Runs splitting method, passes in the reader, writer
                            SplitWavFile(reader, writer, reader.Position, Math.Min(reader.Position + framesPerSecond, totalFrames));
                        }

                    }
                }
                return splitVideosList;
            }
        }

        public void SplitWavFile(WaveFileReader reader, WaveFileWriter writer, long startPos, long endPos)
        {
            {
                reader.Position = startPos;
                // Creates a new buffer with 1024 bytes
                byte[] buffer = new byte[reader.BlockAlign*100];
                while (reader.Position < endPos)
                {

                    // Bytes still left to read
                    int bytesRequired = (int) (endPos - reader.Position);

                    //Console.WriteLine("startpos: " + startPos + " - " + reader.Position);
                    if (bytesRequired > 0)
                    {
                        // Bytes to read next, picks the smallest value of bytesRequired or buffer.length
                        int bytesToRead = Math.Min(bytesRequired, buffer.Length);

                        //if (bytesToRead % reader.BlockAlign != 0) bytesToRead++;
                        // Make sure we dont go out of sync
                        bytesToRead += (bytesToRead%reader.BlockAlign);

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
        }

        public List<string> PlotSpectrogram(List<string> splitMediaInput)
        {
            throw new NotImplementedException();
        }

        public List<string> BandFilter()
        {
            throw new NotImplementedException();
        }

        public List<string> CalculateEnergyBins()
        {
            throw new NotImplementedException();
        }

        public List<string> Normalize()
        {
            throw new NotImplementedException();
        }

        public List<string> CalculateTopTWavelets()
        {
            throw new NotImplementedException();
        }

        public List<string> ComputeWavelets(List<string> spectrogramList)
        {
            throw new NotImplementedException();
        }

        public List<string> HashTransform(List<string> waveletInput)
        {
            throw new NotImplementedException();
        }

        public void PlayMedia(Media media)
        {
            {
                var reader = new MediaFoundationReader(media.FilePath);
                var output = new DirectSoundOut();
                // Initialize reader
                output.Init(reader);
                // Start playback
                output.Play();
                // Dispose after playback
                reader.Dispose();
                output.Dispose();
            }
        }

    }
}
