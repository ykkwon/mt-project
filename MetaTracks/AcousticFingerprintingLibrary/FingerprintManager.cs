using System.Collections.Generic;
using Amazon.DynamoDBv2;
using NAudio.Wave;

namespace AcousticFingerprintingLibrary
{
    public class FingerprintManager : IFingerprintManager
    {
        public Media ConvertToWav(Media inputMedia, Media outputMedia)
        {
            throw new System.NotImplementedException();
        }

        public Media Preprocess(Media inputMedia, int sampleRate)
        {
            throw new System.NotImplementedException();
        }

        public List<string> SplitWavFile(Media inputMedia, Media outputMedia, int overlapFactor, int splitLength)
        {
            throw new System.NotImplementedException();
        }

        public Media SplitWavFile(WaveFileReader reader, WaveFileWriter writer, long startPos, long endPos)
        {
            throw new System.NotImplementedException();
        }

        public List<string> PlotSpectrogram(List<string> splitMediaInput)
        {
            throw new System.NotImplementedException();
        }

        public List<string> CalculateEnergyBins()
        {
            throw new System.NotImplementedException();
        }

        public List<string> CalculateTopTWavelets()
        {
            throw new System.NotImplementedException();
        }

        public List<string> ComputeWavelets(List<string> spectrogramList)
        {
            throw new System.NotImplementedException();
        }

        public List<string> HashTransform(List<string> waveletInput)
        {
            throw new System.NotImplementedException();
        }

        public void SendToDatabase(AmazonDynamoDBClient client, string tableName, string entryName)
        {
            throw new System.NotImplementedException();
        }

        public string DatabaseCompare(AmazonDynamoDBClient client, string hash)
        {
            throw new System.NotImplementedException();
        }
    }
}
