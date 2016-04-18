﻿using System;
using System.Collections.Generic;
using System.Linq;
using AcousticFingerprintingLibrary_0._4._5.FFT;
using AcousticFingerprintingLibrary_0._4._5.Hashing;

namespace AcousticFingerprintingLibrary_0._4._5
{
    public class FingerprintManager
    {
        /// <summary>
        ///   Logarithmic frequency indexes
        /// </summary>
        private int[] _logFrequenciesIndex;

        #region Properties

        private readonly double[] _windowArray;

        /// <summary>
        ///   Window function used in spectrogram computation
        /// </summary>
        public HanningWindow WindowFunction { get; set; }

        /// <summary>
        ///   Wavelet decomposition algorithm
        /// </summary>
        public HaarWavelet HaarWavelet { get; set; }

        /// <summary>
        ///   Number of logarithmically spaced bins between the frequency components computed by FFT.
        /// </summary>
        public int LogBins { get; set; }

        /// <summary>
        ///   Overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        public int Overlap { get; set; }

        /// <summary>
        ///   Size of the window block, 371 ms
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        ///   Frequency range, minimum
        /// </summary>
        public int MinFrequency { get; set; }

        /// <summary>
        ///   Frequency range, maximum
        /// </summary>
        public int MaxFrequency { get; set; }

        /// <summary>
        ///   Number of Top wavelets to consider
        /// </summary>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Sample rate
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        ///   Log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        public double LogBase { get; set; }

        /// <summary>
        ///   Fingerprint's length
        /// </summary>
        public int FingerprintWidth { get; set; }

        public int Stride { get; set; }

        #endregion

        private static int _lshTableSize;
        private static int _lshKey;

        public FingerprintManager()
        {
            _lshTableSize = 25;
            _lshKey = 4;
            WindowFunction = new HanningWindow();
            HaarWavelet = new HaarWavelet();
            LogBins = 32;
            FingerprintWidth = 128;
            Overlap = 64; // Spectrogram overlap
            WindowSize = 2048;
            MinFrequency = 318; // Lowest Frequency
            MaxFrequency = 2000; // Highest Frequency
            TopWavelets = 200;
            SampleRate = 5512;
            LogBase = Math.E;
            Stride = -(Overlap * FingerprintWidth) + 1024;
            _logFrequenciesIndex = GetLogFrequenciesIndex(SampleRate, MinFrequency, MaxFrequency, LogBins, WindowSize,
                LogBase);
            _windowArray = WindowFunction.GetWindow(WindowSize);
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
        public int[] GetLogFrequenciesIndex(int sampleRate, int minFreq, int maxFreq, int logBins, int fftSize,
            double logBase)
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
        private void GenerateLogFrequencies(int sampleRate, int minFreq, int maxFreq, int logBins, int fftSize,
            double logarithmicBase)
        {
            if (_logFrequenciesIndex == null)
            {
                var logMin = Math.Log(minFreq, logarithmicBase);
                var logMax = Math.Log(maxFreq, logarithmicBase);
                var delta = (logMax - logMin)/logBins;

                var indexes = new int[logBins + 1];
                double accDelta = 0;
                for (var i = 0; i <= logBins /*32 octaves*/; ++i)
                {
                    var freq = (float) Math.Pow(logarithmicBase, logMin + accDelta);
                    accDelta += delta; // accDelta = delta * i
                    indexes[i] = FreqToIndex(freq, sampleRate, fftSize);
                        /*Find the start index in array from which to start the summation*/
                }
                _logFrequenciesIndex = indexes;
            }
        }

        /// <summary>
        ///   Gets the index in the spectrogram vector from according to the starting frequency specified as the parameter
        /// </summary>
        /// <param name = "freq">Frequency to be found in the spectrogram vector [E.g. 300Hz]</param>
        /// <param name = "sampleRate">Frequency rate at which the signal was processed [E.g. 5512Hz]</param>
        /// <param name = "spectrumLength">Length of the spectrogram [2048 elements generated by window from which only 1024 are with the actual data]</param>
        /// <returns>Index of the frequency in the spectrogram array</returns>
        /// <remarks>
        ///   The Bandwidth of the spectrogram runs from 0 until SampleRate / 2 [E.g. 5512 / 2]
        ///   Important to remember:
        ///   N points in time domain correspond to N/2 + 1 points in frequency domain
        ///   E.g. 300 Hz applies to 112'th element in the array
        /// </remarks>
        private static int FreqToIndex(float freq, int sampleRate, int spectrumLength)
        {
            var fraction = freq/((float) sampleRate/2);
                /*N sampled points in time correspond to [0, N/2] frequency range */
            var i = (int) Math.Round((spectrumLength/2 + 1)*fraction);
                /*DFT N points defines [N/2 + 1] frequency points*/
            return i;
        }

        #endregion

        #region Spectrograms

        /// <summary>
        ///   Create spectrogram of the input file
        /// Only using in imaging of spectrogram
        /// </summary>
        /// <param name = "filename">Filename</param>
        /// <param name = "milliseconds">Milliseconds to process</param>
        /// <param name = "startmilliseconds">Starting point of the processing</param>
        /// <returns>Spectrogram</returns>
        public float[][] CreateSpectrogram(string filename, int milliseconds, int startmilliseconds)
        {
            //read 5512 Hz, Mono, PCM, with a specific bassproxy
            var samples = BassProxy.ReadMonoFromFileStatic(filename, SampleRate, milliseconds, startmilliseconds);
            NormalizeInPlace(samples);
            var overlap = Overlap;
            var windowSize = WindowSize;
            var width = (samples.Length - windowSize)/overlap; /*width of the image*/
            var frames = new float[width][];
            var fourierSignal = new float[2*windowSize]; /*even - Re, odd - Img*/
            for (var i = 0; i < width; i++)
            {
                //take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (var j = 0; j < windowSize /*2048*/; j++)
                {
                    fourierSignal[2*j] = (float) (_windowArray[j]*samples[i*overlap + j]); /*Weight by Hann Window*/
                    fourierSignal[2*j + 1] = 0;
                }
                //FFT transform for gathering the spectrogram
                Fourier.FFT(fourierSignal, windowSize, FourierDirection.Forward);
                var temp = new float[windowSize/2 + 1];
                for (var j = 0; j < windowSize/2 + 1; j++)
                {
                    double re = fourierSignal[2*j];
                    double img = fourierSignal[2*j + 1];
                    re /= (float) windowSize/2;
                    img /= (float) windowSize/2;
                    temp[j] = (float) Math.Sqrt(re*re + img*img);
                }
                frames[i] = temp;
            }
            return frames;
        }

        /// <summary>
        ///   Create log-spectrogram (spaced according to manager's parameters)
        /// </summary>
        /// <param name = "filename">Filename to be processed</param>
        /// <param name = "milliseconds">Milliseconds to be analyzed</param>
        /// <param name = "startmilliseconds">Starting point</param>
        /// <returns>Logarithmically spaced bins within the power spectrogram</returns>
        public float[][] CreateLogSpectrogram(string filename, int milliseconds, int startmilliseconds)
        {
            //read 5512 Hz, Mono, PCM
            var samples = BassProxy.ReadMonoFromFileStatic(filename, SampleRate, milliseconds, startmilliseconds);
            return CreateLogSpectrogram(samples);
        }

        public float[][] CreateLogSpectrogram(float[] samples)
        {
            NormalizeInPlace(samples);
            var overlap = Overlap;  
            var windowSize = WindowSize;
            var width = (samples.Length - windowSize)/overlap; /*width of the image*/
            var frames = new float[width][];
            var fftSamples = new float[2*windowSize]; /*even - Re, odd - Img*/
            for (var widthIndex = 0; widthIndex < width; widthIndex++)
            {
                //take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (var windowIndex = 0; windowIndex < windowSize; windowIndex++)
                {
                    fftSamples[2*windowIndex] = (float) (_windowArray[windowIndex]*samples[widthIndex*overlap + windowIndex]); /*Weight by Hann Window*/
                    fftSamples[2*windowIndex + 1] = 0;
                }
                //FFT transform for gathering the spectrogram
                Fourier.FFT(fftSamples, windowSize, FourierDirection.Forward);
                frames[widthIndex] = ExtractLogBins(fftSamples);
            }
            return frames;
        }


        // normalize power (volume) of a wave file.
        // minimum and maximum root-mean-square(RMS) to normalize from.
        private const float Minrms = 0.1f;
        private const float Maxrms = 3;

        /// <summary>
        ///   Normalizing volume
        /// </summary>
        /// <param name = "samples">Samples of a file to be normalized</param>
        private static void NormalizeInPlace(IList<float> samples)
        {
            double squares = 0;
            var nsamples = samples.Count;
            for (var i = 0; i < nsamples; i++)
            {
                squares += samples[i]*samples[i];
            }
            // we don't want to normalize by the real RMS, because excessive clipping will occur
            var rms = (float) Math.Sqrt(squares/nsamples)*10;

            if (rms < Minrms)
                rms = Minrms;
            if (rms > Maxrms)
                rms = Maxrms;

            for (var i = 0; i < nsamples; i++)
            {
                samples[i] /= rms;
                samples[i] = Math.Min(samples[i], 1);
                samples[i] = Math.Max(samples[i], -1);
            }
        }

        #endregion

        #region Fingerprinting

        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "filename">Filename to be analyzed</param>
        /// <param name = "milliseconds">Milliseconds to analyze</param>
        /// <param name = "startmilliseconds">Starting point of analysis</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(string filename, int milliseconds, int startmilliseconds)
        {
            var spectrum = CreateLogSpectrogram(filename, milliseconds, startmilliseconds);
            
            return CreateFingerprints(spectrum);
        }

        /// <summary>
        ///   Create fingerprints from already written samples
        /// </summary>
        /// <param name = "samples">Samples from a song</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(float[] samples)
        {
            var spectrum = CreateLogSpectrogram(samples);
            return CreateFingerprints(spectrum);
        }

        /// <summary>
        ///   Create fingerprints gathered from one specific song
        /// </summary>
        /// <param name = "filename">Filename</param>
        /// <returns>List of fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(string filename)
        {
            return CreateFingerprints(filename, 0, 0);
        }

        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "spectrogram">Spectrogram of the song</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(float[][] spectrogram)
        {
            var fingerprintWidth = FingerprintWidth; /*128*/
            var overlap = Overlap; /*64*/
            var fingerprintHeight = LogBins;
            var start = 0;
            var sampleRate = SampleRate;

            var sequenceNr = 0;
            var fingerPrints = new List<Fingerprint>();
            
            var length = spectrogram.GetLength(0);
            while (start + fingerprintWidth <= length)
            {
                var frames = new float[fingerprintWidth][];
                for (var index = 0; index < fingerprintWidth; index++)
                {
                    frames[index] = new float[fingerprintHeight];
                    Array.Copy(spectrogram[start + index], frames[index], fingerprintHeight);
                }

                HaarWavelet.TransformImage(frames); /*Compute wavelets*/
                var image = ExtractTopWavelets(frames);
                fingerPrints.Add(new Fingerprint
                {
                    SequenceNumber = sequenceNr++,
                    Signature = image,
                    Timestamp = start*((double) overlap/sampleRate)
                });
                start += fingerprintWidth + Stride / overlap;
            }
            return fingerPrints;
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
            var logBins = LogBins; /*Local copy for performance reasons*/

            // Safe Code:
            if (spectrum == null)
                throw new ArgumentNullException(nameof(spectrum));
            if (MinFrequency >= MaxFrequency)
                throw new ArgumentException("Minimal frequency cannot be bigger or equal to Maximum frequency");
            if (SampleRate <= 0)
                throw new ArgumentException("sampleRate cannot be less or equal to zero");

            var totalFreq = new float[logBins]; /*32*/
            for (var index = 0; index < logBins; index++)
            {
                var low = _logFrequenciesIndex[index];
                var high = _logFrequenciesIndex[index + 1];

                for (var index2 = low; index2 < high; index2++)
                {
                    double re = spectrum[2*index2];
                    double img = spectrum[2*index2 + 1];
                    totalFreq[index] += (float) (Math.Sqrt(re*re + img*img));
                }
                totalFreq[index] = totalFreq[index]/(high - low);
            }
            return totalFreq;
        }

        #endregion

        #region Wavelet Decomposition

        /// <summary>
        ///   Sets all other wavelet values to 0 except whose which make part of Top Wavelet [top wavelet > 0 ? 1 : -1]
        /// </summary>
        /// <param name = "frames">Frames with 32 logarithmically spaced frequency bins</param>
        /// <returns>Fingerprint signature. Array of encoded Boolean elements (wavelet signature)</returns>
        public bool[] ExtractTopWavelets(float[][] frames)
        {
            var topWavelets = TopWavelets; /*Local copy for performance reasons*/

            // Safe code:
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));
            for (var j = 0; j < frames.GetLength(0); j++)
                if (frames[j] == null)
                    throw new ArgumentNullException(nameof(frames));
            if (topWavelets < 0)
                throw new ArgumentException("numberOfTopWaveletes cannot be less than 0");


            var width = frames.GetLength(0); // 128
            var height = frames[0].Length; // 32

            if (topWavelets >= width*height)
                throw new ArgumentException("TopWaveletes cannot exceed the length of concatenated array");

            var concatenated = new float[width*height]; // 128, 32
            for (var row = 0; row < width; row++)
                Array.Copy(frames[row], 0, concatenated, row*frames[row].Length, frames[row].Length);

            var indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            var abs = new AbsComparator();
            ArraySort(concatenated, indexes, abs);

            //var result = EncodeFingerprint(concatenated, indexes, topWavelets);
            //return result;

            var result = new bool[concatenated.Length * 2]; /*Concatenated float array*/
            for (var i = 0; i < topWavelets; i++)
            {
                var index = indexes[i];
                double value = concatenated[i];
                if (value > 0) /*positive wavelet*/
                    result[index * 2] = true;
                else if (value < 0) /*negative wavelet*/
                    result[index * 2 + 1] = true;
            }
            return result;
        }

        #endregion

        #region Sorting code taken from C# .NET sourcecode

        private void ArraySort(Array keys, Array items, AbsComparator comparer)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);

        }

        public static void Sort(Array keys, Array items, int index, int length, AbsComparator comparer)
        {
            /* //Error handling, fuck that
            if (keys == null)
                throw new ArgumentNullException("keys");
            if (keys.Rank != 1 || (items != null && items.Rank != 1))
                throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            if (items != null && keys.GetLowerBound(0) != items.GetLowerBound(0))
                throw new ArgumentException(Environment.GetResourceString("Arg_LowerBoundsMustMatch"));
            if (index < keys.GetLowerBound(0) || length < 0)
                throw new ArgumentOutOfRangeException((length < 0 ? "length" : "index"), Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (keys.Length - (index - keys.GetLowerBound(0)) < length || (items != null && (index - items.GetLowerBound(0)) > items.Length - length))
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            */

            if (length > 1)
            {
                var objKeys = keys as Object[];
                Object[] objItems = null;
                if (objKeys != null)
                    objItems = items as Object[];
                if (objKeys != null && (items == null || objItems != null))
                {
                    var sorter = new SorterObjectArray(objKeys, objItems, comparer);
                    sorter.QuickSort(index, index + length - 1);
                }
                else
                {
                    var sorter = new SorterGenericArray(keys, items, comparer);
                    sorter.QuickSort(index, index + length - 1);
                }
            }
        }

        private static int GetMedian(int low, int hi)
        {
            return low + ((hi - low) >> 1);
        }

        private struct SorterObjectArray
        {
            private readonly Object[] _keys;
            private readonly Object[] _items;
            private readonly AbsComparator _comparer;

            internal SorterObjectArray(Object[] keys, Object[] items, AbsComparator comparer)
            {
                _keys = keys;
                _items = items;
                _comparer = comparer;
            }

            private void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (_comparer.Compare((float) _keys[a], (float) _keys[b]) > 0)
                    {
                        var temp = _keys[a];
                        _keys[a] = _keys[b];
                        _keys[b] = temp;
                        if (_items != null)
                        {
                            var item = _items[a];
                            _items[a] = _items[b];
                            _items[b] = item;
                        }
                    }
                }
            }


            internal void QuickSort(int left, int right)
            {
                // Can use the much faster jit helpers for array access.
                do
                {
                    var i = left;
                    var j = right;

                    // pre-sort the low, middle (pivot), and high values in place.
                    // this improves performance in the face of already sorted data, or 
                    // data that is made up of multiple sorted runs appended together.
                    var middle = GetMedian(i, j);
                    SwapIfGreaterWithItems(i, middle); // swap the low with the mid point
                    SwapIfGreaterWithItems(i, j); // swap the low with the high 
                    SwapIfGreaterWithItems(middle, j); // swap the middle with the high

                    var x = _keys[middle];
                    do
                    {
                        // Add a try block here to detect IComparers (or their 
                        // underlying IComparables, etc) that are bogus.
                        while (_comparer.Compare((float) _keys[i], (float) x) < 0) i++;
                        while (_comparer.Compare((float) x, (float) _keys[j]) < 0) j--;
                        if (i > j) break;
                        if (i < j)
                        {
                            var key = _keys[i];
                            _keys[i] = _keys[j];
                            _keys[j] = key;
                            if (_items != null)
                            {
                                var item = _items[i];
                                _items[i] = _items[j];
                                _items[j] = item;
                            }
                        }
                        i++;
                        j--;
                    } while (i <= j);
                    if (j - left <= right - i)
                    {
                        if (left < j) QuickSort(left, j);
                        left = i;
                    }
                    else
                    {
                        if (i < right) QuickSort(i, right);
                        right = j;
                    }
                } while (left < right);
            }
        }

        // Private value used by the Sort methods for instances of Array. 
        // This is slower than the one for Object[], since we can't use the JIT helpers 
        // to access the elements.  We must use GetValue & SetValue.
        private struct SorterGenericArray
        {
            private readonly Array _keys;
            private readonly Array _items;
            private readonly AbsComparator _comparer;

            internal SorterGenericArray(Array keys, Array items, AbsComparator comparer)
            {
                _keys = keys;
                _items = items;
                _comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (_comparer.Compare((float) _keys.GetValue(a), (float) _keys.GetValue(b)) > 0)
                    {
                        var key = _keys.GetValue(a);
                        _keys.SetValue(_keys.GetValue(b), a);
                        _keys.SetValue(key, b);
                        if (_items != null)
                        {
                            var item = _items.GetValue(a);
                            _items.SetValue(_items.GetValue(b), a);
                            _items.SetValue(item, b);
                        }
                    }
                }
            }

            internal void QuickSort(int left, int right)
            {
                // Must use slow Array accessors (GetValue & SetValue)
                do
                {
                    var i = left;
                    var j = right;

                    // pre-sort the low, middle (pivot), and high values in place. 
                    // this improves performance in the face of already sorted data, or 
                    // data that is made up of multiple sorted runs appended together.
                    var middle = GetMedian(i, j);
                    SwapIfGreaterWithItems(i, middle); // swap the low with the mid point
                    SwapIfGreaterWithItems(i, j); // swap the low with the high
                    SwapIfGreaterWithItems(middle, j); // swap the middle with the high

                    var x = _keys.GetValue(middle);
                    do
                    {
                        // Add a try block here to detect IComparers (or their 
                        // underlying IComparables, etc) that are bogus.
                        while (_comparer.Compare((float) _keys.GetValue(i), (float) x) < 0) i++;
                        while (_comparer.Compare((float) x, (float) _keys.GetValue(j)) < 0) j--;
                        if (i > j) break;
                        if (i < j)
                        {
                            var key = _keys.GetValue(i);
                            _keys.SetValue(_keys.GetValue(j), i);
                            _keys.SetValue(key, j);
                            if (_items != null)
                            {
                                var item = _items.GetValue(i);
                                _items.SetValue(_items.GetValue(j), i);
                                _items.SetValue(item, j);
                            }
                        }
                        if (i != Int32.MaxValue) ++i;
                        if (j != Int32.MinValue) --j;
                    } while (i <= j);
                    if (j - left <= right - i)
                    {
                        if (left < j) QuickSort(left, j);
                        left = i;
                    }
                    else
                    {
                        if (i < right) QuickSort(i, right);
                        right = j;
                    }
                } while (left < right);
            }
        }

        #endregion
        
        #region Fingerprint Encoding

        /// <summary>
        ///   Encode the integer representation of the fingerprint into Boolean array
        /// </summary>
        /// <param name = "concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name = "indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name = "topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        public static bool[] EncodeFingerprint(float[] concatenated, int[] indexes, int topWavelets)
        {
            //   Negative Numbers = 01
            //   Positive Numbers = 10
            //   Zeros            = 00      
            var result = new bool[concatenated.Length*2]; /*Concatenated float array*/
            for (var i = 0; i < topWavelets; i++)
            {
                var index = indexes[i];
                double value = concatenated[i];
                if (value > 0) /*positive wavelet*/
                    result[index*2] = true;
                else if (value < 0) /*negative wavelet*/
                    result[index*2 + 1] = true;
            }
            return result;
        }

        #endregion

        #region Hashing

        public HashedFingerprint[] GetFingerHashes(List<Fingerprint> listdb)
        {
            var listDb = listdb;
            var minHash = new MinHash();
            var minhashdb = listDb.Select(fing => minHash.ComputeMinHashSignatureByte(fing.Signature)).ToList();
            var lshBuckets = minhashdb.Select(fing => minHash.GroupMinHashToLshBucketsByte(fing, _lshTableSize, _lshKey).Values.ToArray()).ToList();

            //List<HashedFingerprint> hashedFinger = new List<HashedFingerprint>();
            var hashedFinger = new HashedFingerprint[listDb.Count];
            for (var index = 0; index < listDb.Count; index++)
            {
                var hashfinger = new HashedFingerprint(lshBuckets[index], listDb[index].SequenceNumber,
                    listDb[index].Timestamp);
                //hashedFinger.Add(hashfinger);
                hashedFinger[index] = hashfinger;
            }

            return hashedFinger;
        }

        /// <summary>
        /// Takes string values received from databases and converts them to
        /// sorts them into arrays with long[] and double
        /// </summary>
        /// <param name="receivedHashes">Hashbins received from database, will be converted to long[]</param>
        /// <param name="receivedTimestamps">Timestamps received from database, will be converted to double</param>
        /// <returns>
        /// An array of HashedFingerprint objects.
        /// </returns>
        public HashedFingerprint[] GenerateHashedFingerprints(string[] receivedHashes, string[] receivedTimestamps)
        {
            var hashBins = new List<long>();
            var timestamps = new List<double>();

            try
            {
                for (var index = 0; index < receivedHashes.Length - 1; index++)
                {
                    hashBins.Add(Convert.ToInt64(receivedHashes[index]));
                    //timestamps.Add(Convert.ToDouble(receivedTimestamps[index]));
                    timestamps.Add(Math.Round(double.Parse(receivedTimestamps[index].Replace(',', '.'))));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.HResult);
            }

            var hashBinsList = new List<long[]>();
            var timestampList = new List<double>();
            for (var j = 0; j < timestamps.Count - 1; j++)
            {
                if (j % _lshTableSize == 0 && hashBins.Count > j + _lshTableSize)
                {
                    var bins = new long[_lshTableSize];
                    for (var i = 0; i < _lshTableSize; i++)
                    {
                        bins[i] = hashBins[i + j];
                    }
                    hashBinsList.Add(bins);
                    timestampList.Add(timestamps[j]);
                }
            }

            return hashBinsList.Select((t, i) => new HashedFingerprint(t, timestampList[i])).ToArray();
        }

        public List<HashedFingerprint[]> SplitFingerprintLists(HashedFingerprint[] movieFingerprints)
        {
            var chunkSize = 1000;
            List<HashedFingerprint[]> result = new List<HashedFingerprint[]>();
            for (int i = 0; i < movieFingerprints.Length; i += chunkSize)
            {
                var remaining = movieFingerprints.Length - i;
                var iteration = Math.Min(remaining, chunkSize);
                HashedFingerprint[] buffer = new HashedFingerprint[iteration];
                Array.Copy(movieFingerprints, i, buffer, 0, iteration);
                result.Add(buffer);
            }
            return result;
        }

        #endregion

        #region Recognition
        private readonly List<HashedFingerprint> _matchedFingerprints = new List<HashedFingerprint>();
        private HashedFingerprint _bestMatchedFingerprint;
        public bool CompareFingerprintLists(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
        {
            var fingerprintList = fingerprints;
            var toCompareList = toCompare;
            //
            var commonCounter = 0;
            var highestCommon = 0;
            foreach (var fingerprint1 in fingerprintList)
            {
                foreach (var fingerprint2 in toCompareList)
                {
                    //var commonNumbers = fingerprint1.HashBins.Intersect(fingerprint2.HashBins);

                    HashSet<long> set2 = new HashSet<long>(fingerprint2.HashBins); // 7643
                    var i = fingerprint1.HashBins.Select(hash => set2.Contains(hash)).Count(qwe => qwe);

                    var count = i;
                    if (count >= 4)
                    {
                        _matchedFingerprints.Add(fingerprint1);
                        // Best matched fingerprint is the fingerprint with the highest number of hashes being equal to original fingerprint
                        if (highestCommon <= count)
                        {
                            _bestMatchedFingerprint = fingerprint1;
                            highestCommon = count;
                        }
                        // potential match
                        commonCounter++;
                        break; // jumps out of loop and on to next fingerprint
                    }
                }
            }
            // If result is greater than 5% it is a potential match
            var result = (double) (100 * commonCounter)/fingerprints.Length;
            return result > 5; // if result greater than 5, return true, else false
        }

        // Get the newest timeStamp found in recognition
        // Call this to get the newest timestamp found
        public static double LatestTimeStamp { get; set; }
        public double CompareFingerprintListsHighest(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
        {
            foreach (var fingerprint1 in fingerprints)
            {
                foreach (var fingerprint2 in toCompare)
                {
                    //var commonNumbers = fingerprint1.HashBins.Intersect(fingerprint2.HashBins);

                    HashSet<long> set2 = new HashSet<long>(fingerprint2.HashBins); // 7643
                    var i = fingerprint1.HashBins.Select(hash => set2.Contains(hash)).Count(qwe => qwe);

                    var count = i;
                    if (count >= 4)
                    {
                        _matchedFingerprints.Add(fingerprint1);
                        // Sets updates LatestFingerprint with 
                        LatestTimeStamp = fingerprint1.Timestamp;
                        Console.WriteLine("LAST MATCHED: " + fingerprint1.Timestamp);
                        break; // jumps out of loop and on to next fingerprint
                    }
                }
            }

            
            // If result is greater than 5% it is a potential match
            //var result = (double)(100 * commonCounter) / fingerprints.Length;
            return _matchedFingerprints.Count; // if result greater than 5, return true, else false
        }

        public double GetTimeStamps(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
        {
            if (CompareFingerprintLists(fingerprints, toCompare))
            {
                /*
                // returns index of the best fingerprint matched
                var max = _bestMatchedFingerprint.Max();
                // index of the fingerprint with highest number of matches
                var index = _bestMatchedFingerprint.IndexOf(max) - 1;
                */

                // returns timestamp of matched fingerprint
                return _bestMatchedFingerprint.Timestamp;
                //return _matchedFingerprints.Last().Timestamp;
            }
            // Returns -1 if the lists are not a match
            return -1;
        }

        /// <summary>
        /// Searches through all fingerprints from movie to find the section with most correct fingerprints.
        /// This is for faster searching later on
        /// </summary>
        /// <param name="allFingerprints">List returned from of fingerprints in SplitFingerprintLists() method</param>
        /// <param name="toCompare">Microphone recording</param>
        /// <returns>Index of the list with most matched fingerprints.</returns>
        public int FindBestFingerprintList(List<HashedFingerprint[]> allFingerprints, HashedFingerprint[] toCompare)
        {
            int bestIndex = -1;
            for (var index = 0; index < allFingerprints.Count; index++)
            {
                var list = allFingerprints[index];
                var fingerprints = CompareFingerprintListsHighest(list, toCompare);
                if (fingerprints > bestIndex)
                    bestIndex = index;
                _matchedFingerprints.Clear();
            }
            return bestIndex;
        }

        private int _counter;
        private double _previoustimestamp;
        public bool CheckIteration(double timestamp, HashedFingerprint[] nextIteration)
        {
            // if next starts at 100 and timestamp reaches 90+
            // 90-100 = -10... correct i thinkkk
            if (timestamp - nextIteration[0].Timestamp >= -10)
            {
                _counter++;
                if (_counter >= 2 && Math.Abs(timestamp - _previoustimestamp) > 0)
                {
                    _counter = 0;
                    _previoustimestamp = 0.0;
                    return true;
                }
                _previoustimestamp = timestamp;
            }
            return false;
        }

        #endregion
        /// <summary>
        ///   Absolute value comparator
        /// </summary>
        public class AbsComparator : IComparer<float>
        {
            #region IComparer<float> Members

            /// <summary>
            ///   Compare descending
            /// </summary>
            /// <returns>Return details related to magnitude comparison</returns>
            public int Compare(float x, float y)
            {
                // Math.Abs(y).CompareTo(Math.Abs(x)); returns -1 or 1
                // If X is bigger, return -1, if Y is bigger return 1
                return Math.Abs(y).CompareTo(Math.Abs(x));
            }

            #endregion
        }
    }
}
