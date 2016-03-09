using System.Collections.Generic;
using Amazon.DynamoDBv2;
using NAudio.Wave;

namespace AcousticFingerprintingLibrary
{
    public interface IFingerprintManager
    {
        // Todo: Add checks, sanitation and unit tests
        /// <summary>
        /// Will convert input Media to an uncompressed Wave format. 
        /// </summary>
        /// <param name="inputMedia">Input Media (.mp4 or .avi)</param>
        /// <returns></returns>
        Media ConvertToWav(Media inputMedia);

        // Todo: Add checks, sanitation and unit tests 
        /// <summary>
        /// Will flatten channels (surround to mono) and downsample the input media.
        /// </summary>
        /// <param name="inputMedia">Input Media (wave .wav)</param>
        /// <param name="sampleRate">Output sample rate</param>
        /// <returns></returns>
        Media Preprocess(Media inputMedia, int sampleRate);

        // Todo: Remove?
        /// <summary>
        /// Split the input Media into equal parts of splitLength. 
        /// </summary>
        /// <param name="inputMedia">Input Media (wave .wav)</param>
        /// <param name="outputMedia">Output Media (wave .wav)</param>
        /// <param name="overlapFactor">Overlap factor for split output</param>
        /// <param name="splitLength">Split length in milliseconds</param>
        /// <returns></returns>
        List<string> SplitWavFile(Media inputMedia, Media outputMedia, int overlapFactor, int splitLength);

        // Todo: Remove?
        /// <summary>
        /// Execute the actual split.
        /// </summary>
        /// <param name="reader">Wave file reader</param>
        /// <param name="writer">Wave file writer</param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        void SplitWavFile(WaveFileReader reader, WaveFileWriter writer, long startPos, long endPos);

        // Todo: 
        /// <summary>
        /// Carry out STFT (Short-Time Fourier Transform) and plot the accompanying spectrogram for each file in splitMediaInput.
        /// </summary>
        /// <param name="splitMediaInput">Input media list</param>
        /// <returns>List with file paths to the spectrogram pictures</returns>
        List<string> PlotSpectrogram(List<string> splitMediaInput);

        // Todo:
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> BandFilter();

        // Todo: 
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> CalculateEnergyBins();
        
        // Todo:
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> Normalize();

            // Todo: 
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> CalculateTopTWavelets();

        // Todo: 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrogramList"></param>
        /// <returns></returns>
        List<string> ComputeWavelets(List<string> spectrogramList);

        // Todo: 
        /// <summary>
        ///  Hash all wavelets in waveletInput.
        /// </summary>
        /// <param name="waveletInput"></param>
        /// <returns></returns>
        List<string> HashTransform(List<string> waveletInput);

        // Todo: Add checks, sanitation and unit tests
        /// <summary>
        /// 
        /// </summary>
        void PlayMedia(Media media);
    }
}