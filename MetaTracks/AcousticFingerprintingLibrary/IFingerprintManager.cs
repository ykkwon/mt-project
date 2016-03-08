using System.Collections.Generic;
using Amazon.DynamoDBv2;
using NAudio.Wave;

namespace AcousticFingerprintingLibrary
{
    public interface IFingerprintManager
    {
        /// <summary>
        /// Will convert input Media to an uncompressed Wave format. 
        /// </summary>
        /// <param name="inputMedia">Input Media (.mp4 or .avi)</param>
        /// <param name="outputMedia">Output Media (wave .wav)</param>
        /// <returns></returns>
        Media ConvertToWav(Media inputMedia);

        /// <summary>
        /// Will flatten channels (surround to mono) and downsample the input media.
        /// </summary>
        /// <param name="inputMedia">Input Media (wave .wav)</param>
        /// <param name="sampleRate">Output sample rate</param>
        /// <returns></returns>
        Media Preprocess(Media inputMedia, int sampleRate);

        /// <summary>
        /// Split the input Media into equal parts of splitLength. 
        /// </summary>
        /// <param name="inputMedia">Input Media (wave .wav)</param>
        /// <param name="outputMedia">Output Media (wave .wav)</param>
        /// <param name="overlapFactor">Overlap factor for split output</param>
        /// <param name="splitLength">Split length in milliseconds</param>
        /// <returns></returns>
        List<string> SplitWavFile(Media inputMedia, Media outputMedia, int overlapFactor, int splitLength);

        /// <summary>
        /// Execute the actual split.
        /// </summary>
        /// <param name="reader">Wave file reader</param>
        /// <param name="writer">Wave file writer</param>
        /// <returns></returns>
        void SplitWavFile(WaveFileReader reader, WaveFileWriter writer, long startPos, long endPos);

        /// <summary>
        /// Carry out STFT (Short-Time Fourier Transform) and plot the accompanying spectrogram for each file in splitMediaInput.
        /// </summary>
        /// <param name="splitMediaInput">Input media list</param>
        /// <returns>List with file paths to the spectrogram pictures</returns>
        List<string> PlotSpectrogram(List<string> splitMediaInput);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> CalculateEnergyBins();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> CalculateTopTWavelets();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrogramList"></param>
        /// <returns></returns>
        List<string> ComputeWavelets(List<string> spectrogramList);

        /// <summary>
        ///  Hash all wavelets in waveletInput.
        /// </summary>
        /// <param name="waveletInput"></param>
        /// <returns></returns>
        List<string> HashTransform(List<string> waveletInput);

        /// <summary>
        /// Send hashes to database
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tableName"></param>
        /// <param name="entryName"></param>
        void SendToDatabase(AmazonDynamoDBClient client, string tableName, string entryName);

        /// <summary>
        /// Returns a strint containing positional data and playback data if it exists in the database
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        string DatabaseCompare(AmazonDynamoDBClient client, string hash);
    }
}