﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary.SoundFingerprint.FFT;
using AcousticFingerprintingLibrary.SoundFingerprint.Wavelets;
using AcousticFingerprintingLibrary.SoundFingerprint.Windows;

namespace AcousticFingerprintingLibrary.SoundFingerprint
{
    public class Fingerprinter
    {
        /// <summary>
        ///   Logarithmic frequency indexes
        /// </summary>
        private int[] _logFrequenciesIndex;


        /// <summary>
        ///   Human auditory threshold
        /// </summary>
        public const double HUMAN_AUDITORY_THRESHOLD = 2 * 0.000001; /*2*10^-5 Pa*/

        #region Properties

        private readonly double[] _windowArray;

        /// <summary>
        ///   Window function used in spectrogram computation
        /// </summary>
        /// <remarks>
        ///   Default <c>HanningWindow</c>
        /// </remarks>
        public IWindowFunction WindowFunction { get; set; }

        /// <summary>
        ///   Wavelet decomposition algorithm
        /// </summary>
        /// <remarks>
        ///   Default <c>HaarWavelet</c>
        /// </remarks>
        public IWaveletDecomposition WaveletDecomposition { get; set; }

        /// <summary>
        ///   Number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        /// <remarks>
        ///   Default = 32
        /// </remarks>
        public int LogBins { get; set; }

        /// <summary>
        ///   Number of samples to read in order to create single fingerprint.
        ///   The granularity is 1.48 seconds
        /// </summary>
        /// <remarks>
        ///   Default = 8192
        /// </remarks>
        public int SamplesPerFingerprint { get; set; }

        /// <summary>
        ///   Overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        /// <remarks>
        ///   Default = 64
        /// </remarks>
        public int Overlap { get; set; }

        /// <summary>
        ///   Size of the WDFT block, 371 ms
        /// </summary>
        /// <remarks>
        ///   Default = 2048
        /// </remarks>
        public int WdftSize { get; set; }

        /// <summary>
        ///   Frequency range which is taken into account
        /// </summary>
        /// <remarks>
        ///   Default = 318
        /// </remarks>
        public int MinFrequency { get; set; }

        /// <summary>
        ///   Frequency range which is taken into account
        /// </summary>
        /// <remarks>
        ///   Default = 2000
        /// </remarks>
        public int MaxFrequency { get; set; }

        /// <summary>
        ///   Number of Top wavelets to consider
        /// </summary>
        /// <remarks>
        ///   Default = 200
        /// </remarks>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Sample rate
        /// </summary>
        /// <remarks>
        ///   Default = 5512
        /// </remarks>
        public int SampleRate { get; set; }

        /// <summary>
        ///   Log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        /// <remarks>
        ///   Default = 10
        /// </remarks>
        public double LogBase { get; set; }

        /// <summary>
        ///   Fingerprint's length
        /// </summary>
        public int FingerprintLength { get; set; }

        #endregion

        public Fingerprinter()
        {
            WindowFunction = new HanningWindow();
            WaveletDecomposition = new HaarWavelet();
            LogBins = 32;
            FingerprintLength = 128;
            Overlap = 64;
            SamplesPerFingerprint = FingerprintLength * Overlap;
            WdftSize = 2048;
            MinFrequency = 318;
            MaxFrequency = 2000;
            TopWavelets = 200;
            SampleRate = 5512;
            LogBase = Math.E;
            _logFrequenciesIndex = GetLogFrequenciesIndex(SampleRate, MinFrequency, MaxFrequency, LogBins, WdftSize, LogBase);
            _windowArray = WindowFunction.GetWindow(WdftSize);
        }
        #region LogFrequencies

        /// <summary>
        ///   Get logarithmically spaced indices
        /// </summary>
        /// <param name = "sampleRate">Signal's sample rate</param>
        /// <param name = "minFreq">Min frequency</param>
        /// <param name = "maxFreq">Max frequency</param>
        /// <param name = "logBins">Number of logarithmically spaced bins</param>
        /// <param name = "fftSize">FFT Size</param>
        /// <param name = "logBase">Log base of the logarithm to be spaced</param>
        /// <returns>Gets an array of indexes</returns>
        public int[] GetLogFrequenciesIndex(int sampleRate, int minFreq, int maxFreq, int logBins, int fftSize, double logBase)
        {
            if (_logFrequenciesIndex == null)
                GenerateLogFrequencies(sampleRate, minFreq, maxFreq, logBins, fftSize, logBase);
            return _logFrequenciesIndex;
        }

        /// <summary>
        ///   Get logarithmically spaced indices
        /// </summary>
        /// <param name = "sampleRate">Signal's sample rate</param>
        /// <param name = "minFreq">Min frequency</param>
        /// <param name = "maxFreq">Max frequency</param>
        /// <param name = "logBins">Number of logarithmically spaced bins</param>
        /// <param name = "fftSize">FFT Size</param>
        /// <param name = "logarithmicBase">Logarithm base</param>
        private void GenerateLogFrequencies(int sampleRate, int minFreq, int maxFreq, int logBins, int fftSize, double logarithmicBase)
        {
            if (_logFrequenciesIndex == null)
            {
                double logMin = Math.Log(minFreq, logarithmicBase);
                double logMax = Math.Log(maxFreq, logarithmicBase);
                double delta = (logMax - logMin) / logBins;

                int[] indexes = new int[logBins + 1];
                double accDelta = 0;
                for (int i = 0; i <= logBins /*32 octaves*/; ++i)
                {
                    float freq = (float)Math.Pow(logarithmicBase, logMin + accDelta);
                    accDelta += delta; // accDelta = delta * i
                    indexes[i] = FreqToIndex(freq, sampleRate, fftSize); /*Find the start index in array from which to start the summation*/
                }
                _logFrequenciesIndex = indexes;
            }
        }

        /// <summary>
        ///   Gets the index in the spectrum vector from according to the starting frequency specified as the parameter
        /// </summary>
        /// <param name = "freq">Frequency to be found in the spectrum vector [E.g. 300Hz]</param>
        /// <param name = "sampleRate">Frequency rate at which the signal was processed [E.g. 5512Hz]</param>
        /// <param name = "spectrumLength">Length of the spectrum [2048 elements generated by WDFT from which only 1024 are with the actual data]</param>
        /// <returns>Index of the frequency in the spectrum array</returns>
        /// <remarks>
        ///   The Bandwidth of the spectrum runs from 0 until SampleRate / 2 [E.g. 5512 / 2]
        ///   Important to remember:
        ///   N points in time domain correspond to N/2 + 1 points in frequency domain
        ///   E.g. 300 Hz applies to 112'th element in the array
        /// </remarks>
        private static int FreqToIndex(float freq, int sampleRate, int spectrumLength)
        {
            float fraction = freq / ((float)sampleRate / 2); /*N sampled points in time correspond to [0, N/2] frequency range */
            int i = (int)Math.Round((spectrumLength / 2 + 1) * fraction); /*DFT N points defines [N/2 + 1] frequency points*/
            return i;
        }
        #endregion

        #region Spectrograms

        /// <summary>
        ///   Create spectrogram of the input file
        /// </summary>
        /// <param name = "proxy">Proxy used to read from file</param>
        /// <param name = "filename">Filename</param>
        /// <param name = "milliseconds">Milliseconds to process</param>
        /// <param name = "startmilliseconds">Starting point of the processing</param>
        /// <returns>Spectrogram</returns>
        public float[][] CreateSpectrogram(IAudio proxy, string filename, int milliseconds, int startmilliseconds)
        {
            //read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = proxy.ReadMonoFromFile(filename, SampleRate, milliseconds, startmilliseconds);
            NormalizeInPlace(samples);
            int overlap = Overlap;
            int wdftSize = WdftSize;
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                //take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[2 * j] = (float)(_windowArray[j] * samples[i * overlap + j]); /*Weight by Hann Window*/
                    complexSignal[2 * j + 1] = 0;
                }
                //FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                float[] band = new float[wdftSize / 2 + 1];
                for (int j = 0; j < wdftSize / 2 + 1; j++)
                {
                    double re = complexSignal[2 * j];
                    double img = complexSignal[2 * j + 1];
                    re /= (float)wdftSize / 2;
                    img /= (float)wdftSize / 2;
                    band[j] = (float)Math.Sqrt(re * re + img * img);
                }
                frames[i] = band;
            }
            return frames;
        }

        /// <summary>
        ///   Create log-spectrogram (spaced according to manager's parameters)
        /// </summary>
        /// <param name = "proxy">Proxy used in generating the spectrogram</param>
        /// <param name = "filename">Filename to be processed</param>
        /// <param name = "milliseconds">Milliseconds to be analyzed</param>
        /// <param name = "startmilliseconds">Starting point</param>
        /// <returns>Logarithmically spaced bins within the power spectrum</returns>
        public float[][] CreateLogSpectrogram(IAudio proxy, string filename, int milliseconds, int startmilliseconds)
        {
            //read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = proxy.ReadMonoFromFile(filename, SampleRate, milliseconds, startmilliseconds);
            return CreateLogSpectrogram(samples);
        }

        /// <summary>
        ///   Create logarithmically spaced spectrogram out of the input samples (spaced according to manager's parameters)
        /// </summary>
        /// <param name = "samples">Samples of a song</param>
        /// <returns>Logarithmically spaced bins within the power spectrum</returns>
        public float[][] CreateLogSpectrogram(float[] samples)
        {
            NormalizeInPlace(samples);
            int overlap = Overlap;
            int wdftSize = WdftSize;
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                //take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[2 * j] = (float)(_windowArray[j] * samples[i * overlap + j]); /*Weight by Hann Window*/
                    complexSignal[2 * j + 1] = 0;
                }
                //FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                frames[i] = ExtractLogBins(complexSignal);
            }
            return frames;
        }


        // normalize power (volume) of a wave file.
        // minimum and maximum rms to normalize from.
        private const float MINRMS = 0.1f;
        private const float MAXRMS = 3;
        /// <summary>
        ///   Normalizing the input power (volume)
        /// </summary>
        /// <param name = "samples">Samples of a song to be normalized</param>
        private static void NormalizeInPlace(float[] samples)
        {
            double squares = 0;
            int nsamples = samples.Length;
            for (int i = 0; i < nsamples; i++)
            {
                squares += samples[i] * samples[i];
            }
            // we don't want to normalize by the real RMS, because excessive clipping will occur
            float rms = (float)Math.Sqrt(squares / nsamples) * 10;

            if (rms < MINRMS)
                rms = MINRMS;
            if (rms > MAXRMS)
                rms = MAXRMS;

            for (int i = 0; i < nsamples; i++)
            {
                samples[i] /= rms;
                samples[i] = Math.Min(samples[i], 1);
                samples[i] = Math.Max(samples[i], -1);
            }
        }
        #endregion
        

        #region Frequency Manipulation

        /// <summary>
        ///   Logarithmic spacing of a frequency in a linear domain
        /// </summary>
        /// <param name = "spectrum">Spectrum to space</param>
        /// <returns>Logarithmically spaced signal</returns>
        public float[] ExtractLogBins(float[] spectrum)
        {
            int logBins = LogBins; /*Local copy for performance reasons*/
            #if SAFE
            if (spectrum == null)
                throw new ArgumentNullException("spectrum");
            if (MinFrequency >= MaxFrequency)
                throw new ArgumentException("Minimal frequency cannot be bigger or equal to Maximum frequency");
            if (SampleRate <= 0)
                throw new ArgumentException("sampleRate cannot be less or equal to zero");
            #endif
            int wdftSize = WdftSize;
            float[] sumFreq = new float[logBins]; /*32*/
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = _logFrequenciesIndex[i];
                int hiBound = _logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < hiBound; k++)
                {
                    double re = spectrum[2 * k];
                    double img = spectrum[2 * k + 1];
                    //re /= (float) wdftSize / 2;    //normalize img/re part
                    //img /= (float)wdftSize / 2;    //doesn't introduce any change in final image (linear normalization)
                    sumFreq[i] += (float)(Math.Sqrt(re * re + img * img));
                }
                sumFreq[i] = sumFreq[i] / (hiBound - lowBound);
            }
            return sumFreq;
        }

        #endregion
    }
}
