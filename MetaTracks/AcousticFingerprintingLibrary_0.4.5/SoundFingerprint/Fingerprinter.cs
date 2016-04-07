using System;
using System.Collections.Generic;
using System.Linq;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.FFT;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.Hashing;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.Wavelets;
using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.Windows;

namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint
{
    public class Fingerprinter
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
        public IWindowFunction WindowFunction { get; set; }

        /// <summary>
        ///   Wavelet decomposition algorithm
        /// </summary>
        public IWaveletDecomposition WaveletDecomposition { get; set; }

        /// <summary>
        ///   Number of logarithmically spaced bins between the frequency components computed by FFT.
        /// </summary>
        public int LogBins { get; set; }

        /// <summary>
        ///   Number of samples to read in order to create single fingerprint.
        ///   The granularity is 1.48 seconds
        /// </summary>
        public int SamplesPerFingerprint { get; set; }

        /// <summary>
        ///   Overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        public int Overlap { get; set; }

        /// <summary>
        ///   Size of the WDFT block, 371 ms
        /// </summary>
        public int WdftSize { get; set; }

        /// <summary>
        ///   Frequency range which is taken into account
        /// </summary>
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
        public int FingerprintLength { get; set; }

        public int StrideSize { get; set; }

        #endregion

        public Fingerprinter()
        {
            WindowFunction = new HanningWindow();
            WaveletDecomposition = new HaarWavelet();
            LogBins = 32;
            FingerprintLength = 128;
            Overlap = 64;
            SamplesPerFingerprint = FingerprintLength*Overlap;
            WdftSize = 2048;
            MinFrequency = 318;
            MaxFrequency = 2000;
            TopWavelets = 200;
            SampleRate = 5512;
            LogBase = Math.E;
            StrideSize = 1102;
            _logFrequenciesIndex = GetLogFrequenciesIndex(SampleRate, MinFrequency, MaxFrequency, LogBins, WdftSize,
                LogBase);
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
                double logMin = Math.Log(minFreq, logarithmicBase);
                double logMax = Math.Log(maxFreq, logarithmicBase);
                double delta = (logMax - logMin)/logBins;

                int[] indexes = new int[logBins + 1];
                double accDelta = 0;
                for (int i = 0; i <= logBins /*32 octaves*/; ++i)
                {
                    float freq = (float) Math.Pow(logarithmicBase, logMin + accDelta);
                    accDelta += delta; // accDelta = delta * i
                    indexes[i] = FreqToIndex(freq, sampleRate, fftSize);
                        /*Find the start index in array from which to start the summation*/
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
            float fraction = freq/((float) sampleRate/2);
                /*N sampled points in time correspond to [0, N/2] frequency range */
            int i = (int) Math.Round((spectrumLength/2 + 1)*fraction);
                /*DFT N points defines [N/2 + 1] frequency points*/
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
        public float[][] CreateSpectrogram(BassProxy proxy, string filename, int milliseconds, int startmilliseconds)
        {
            //read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = proxy.ReadMonoFromFile(filename, SampleRate, milliseconds, startmilliseconds);
            NormalizeInPlace(samples);
            int overlap = Overlap;
            int wdftSize = WdftSize;
            int width = (samples.Length - wdftSize)/overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2*wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                //take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[2*j] = (float) (_windowArray[j]*samples[i*overlap + j]); /*Weight by Hann Window*/
                    complexSignal[2*j + 1] = 0;
                }
                //FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                float[] band = new float[wdftSize/2 + 1];
                for (int j = 0; j < wdftSize/2 + 1; j++)
                {
                    double re = complexSignal[2*j];
                    double img = complexSignal[2*j + 1];
                    re /= (float) wdftSize/2;
                    img /= (float) wdftSize/2;
                    band[j] = (float) Math.Sqrt(re*re + img*img);
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
        /// <param name="stride">Byte length of fingerprint</param>
        /// <returns>Logarithmically spaced bins within the power spectrum</returns>
        public float[][] CreateLogSpectrogram(BassProxy proxy, string filename, int milliseconds, int startmilliseconds,
            IStride stride)
        {
            if (stride == null) throw new ArgumentNullException(nameof(stride));

            //read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = proxy.ReadMonoFromFile(filename, SampleRate, milliseconds, startmilliseconds);
            return CreateLogSpectrogram(samples, stride);
        }

        public float[][] CreateLogSpectrogram(float[] samples, IStride stride)
        {
            NormalizeInPlace(samples);
            int overlap = Overlap;
            int wdftSize = WdftSize;
            int width = (samples.Length - wdftSize)/overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2*wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                //take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (var j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[2*j] = (float) (_windowArray[j]*samples[i*overlap + j]); /*Weight by Hann Window*/
                    complexSignal[2*j + 1] = 0;
                }
                //FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                frames[i] = ExtractLogBins(complexSignal);
            }
            return frames;
        }


        // normalize power (volume) of a wave file.
        // minimum and maximum rms to normalize from.
        private const float Minrms = 0.1f;
        private const float Maxrms = 3;

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
                squares += samples[i]*samples[i];
            }
            // we don't want to normalize by the real RMS, because excessive clipping will occur
            var rms = (float) Math.Sqrt(squares/nsamples)*10;

            if (rms < Minrms)
                rms = Minrms;
            if (rms > Maxrms)
                rms = Maxrms;

            for (int i = 0; i < nsamples; i++)
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
        /// <param name = "proxy">Proxy used in reading from file</param>
        /// <param name = "filename">Filename to be analyzed</param>
        /// <param name = "stride">Stride between 2 consecutive fingerprints</param>
        /// <param name = "milliseconds">Milliseconds to analyze</param>
        /// <param name = "startmilliseconds">Starting point of analysis</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(BassProxy proxy, string filename, IStride stride, int milliseconds,
            int startmilliseconds)
        {
            var spectrum = CreateLogSpectrogram(proxy, filename, milliseconds, startmilliseconds, stride);
            
            return CreateFingerprints(spectrum, stride);
        }

        /// <summary>
        ///   Create fingerprints from already written samples
        /// </summary>
        /// <param name = "samples">Samples from a song</param>
        /// <param name = "stride">Stride between 2 consecutive fingerprints</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(float[] samples, IStride stride)
        {
            float[][] spectrum = CreateLogSpectrogram(samples, stride);
            return CreateFingerprints(spectrum, stride);
        }

        /// <summary>
        ///   Create fingerprints gathered from one specific song
        /// </summary>
        /// <param name = "proxy">Proxy used in reading the audio file</param>
        /// <param name = "filename">Filename</param>
        /// <param name = "stride">Stride used in fingerprint creation</param>
        /// <returns>List of fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(BassProxy proxy, string filename, IStride stride)
        {
            return CreateFingerprints(proxy, filename, stride, 0, 0);
        }

        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "spectrum">Spectrogram of the song</param>
        /// <param name = "stride">Stride between 2 consecutive fingerprints</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(float[][] spectrum, IStride stride)
        {
            int fingerprintLength = FingerprintLength; /*128*/
            int overlap = Overlap; /*64*/
            int logbins = LogBins;
            int start = stride.GetFirstStride()/overlap;
            int sampleRate = SampleRate;

            int sequenceNumber = 0;
            var fingerPrints = new List<Fingerprint>();

            List<bool[]> fingerprints = new List<bool[]>();

            int width = spectrum.GetLength(0);
            while (start + fingerprintLength <= width)
            {
                float[][] frames = new float[fingerprintLength][];
                for (int i = 0; i < fingerprintLength; i++)
                {
                    frames[i] = new float[logbins];
                    Array.Copy(spectrum[start + i], frames[i], logbins);
                }

                WaveletDecomposition.DecomposeImageInPlace(frames); /*Compute wavelets*/

                bool[] image = ExtractTopWavelets(frames);
                fingerprints.Add(image);
                fingerPrints.Add(new Fingerprint
                {
                    SequenceNumber = ++sequenceNumber,
                    Signature = image,
                    Timestamp = start*((double) overlap/sampleRate)
                });
                var strid = stride.GetStride(); //1102, prøve -7090?
                start += fingerprintLength + (strid/overlap);
            }
            //return fingerprints;
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
            int logBins = LogBins; /*Local copy for performance reasons*/

            // Safe Code:
            if (spectrum == null)
                throw new ArgumentNullException("spectrum");
            if (MinFrequency >= MaxFrequency)
                throw new ArgumentException("Minimal frequency cannot be bigger or equal to Maximum frequency");
            if (SampleRate <= 0)
                throw new ArgumentException("sampleRate cannot be less or equal to zero");

            float[] sumFreq = new float[logBins]; /*32*/
            for (var i = 0; i < logBins; i++)
            {
                var lowBound = _logFrequenciesIndex[i];
                var hiBound = _logFrequenciesIndex[i + 1];

                for (var index = lowBound; index < hiBound; index++)
                {
                    double re = spectrum[2*index];
                    double img = spectrum[2*index + 1];
                    sumFreq[i] += (float) (Math.Sqrt(re*re + img*img));
                }
                sumFreq[i] = sumFreq[i]/(hiBound - lowBound);
            }
            return sumFreq;
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
            int topWavelets = TopWavelets; /*Local copy for performance reasons*/

            // Safe code:
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));
            for (int j = 0; j < frames.GetLength(0); j++)
                if (frames[j] == null)
                    throw new ArgumentNullException(nameof(frames));
            if (topWavelets < 0)
                throw new ArgumentException("numberOfTopWaveletes cannot be less than 0");


            int width = frames.GetLength(0); // 128
            int height = frames[0].Length; // 32

            if (topWavelets >= width*height)
                throw new ArgumentException("TopWaveletes cannot exceed the length of concatenated array");

            float[] concatenated = new float[width*height]; // 128, 32
            for (int row = 0; row < width; row++)
                Array.Copy(frames[row], 0, concatenated, row*frames[row].Length, frames[row].Length);

            Int32[] indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            AbsComparator abs = new AbsComparator();
            ArraySort(concatenated, indexes, abs);

            bool[] result = EncodeFingerprint(concatenated, indexes, topWavelets);
            return result;
        }

        #endregion

        #region Sorting code taken from C# .NET sourcecode

        private void ArraySort(Array keys, Array items, AbsComparator comparer)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

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
                Object[] objKeys = keys as Object[];
                Object[] objItems = null;
                if (objKeys != null)
                    objItems = items as Object[];
                if (objKeys != null && (items == null || objItems != null))
                {
                    SorterObjectArray sorter = new SorterObjectArray(objKeys, objItems, comparer);
                    sorter.QuickSort(index, index + length - 1);
                }
                else
                {
                    SorterGenericArray sorter = new SorterGenericArray(keys, items, comparer);
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
            private Object[] keys;
            private Object[] items;
            private AbsComparator comparer;

            internal SorterObjectArray(Object[] keys, Object[] items, AbsComparator comparer)
            {
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (comparer.Compare((float) keys[a], (float) keys[b]) > 0)
                    {
                        Object temp = keys[a];
                        keys[a] = keys[b];
                        keys[b] = temp;
                        if (items != null)
                        {
                            Object item = items[a];
                            items[a] = items[b];
                            items[b] = item;
                        }
                    }
                }
            }


            internal void QuickSort(int left, int right)
            {
                // Can use the much faster jit helpers for array access.
                do
                {
                    int i = left;
                    int j = right;

                    // pre-sort the low, middle (pivot), and high values in place.
                    // this improves performance in the face of already sorted data, or 
                    // data that is made up of multiple sorted runs appended together.
                    int middle = GetMedian(i, j);
                    SwapIfGreaterWithItems(i, middle); // swap the low with the mid point
                    SwapIfGreaterWithItems(i, j); // swap the low with the high 
                    SwapIfGreaterWithItems(middle, j); // swap the middle with the high

                    Object x = keys[middle];
                    do
                    {
                        // Add a try block here to detect IComparers (or their 
                        // underlying IComparables, etc) that are bogus.
                        while (comparer.Compare((float) keys[i], (float) x) < 0) i++;
                        while (comparer.Compare((float) x, (float) keys[j]) < 0) j--;
                        if (i > j) break;
                        if (i < j)
                        {
                            Object key = keys[i];
                            keys[i] = keys[j];
                            keys[j] = key;
                            if (items != null)
                            {
                                Object item = items[i];
                                items[i] = items[j];
                                items[j] = item;
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
            private Array keys;
            private Array items;
            private AbsComparator comparer;

            internal SorterGenericArray(Array keys, Array items, AbsComparator comparer)
            {
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (comparer.Compare((float) keys.GetValue(a), (float) keys.GetValue(b)) > 0)
                    {
                        Object key = keys.GetValue(a);
                        keys.SetValue(keys.GetValue(b), a);
                        keys.SetValue(key, b);
                        if (items != null)
                        {
                            Object item = items.GetValue(a);
                            items.SetValue(items.GetValue(b), a);
                            items.SetValue(item, b);
                        }
                    }
                }
            }

            internal void QuickSort(int left, int right)
            {
                // Must use slow Array accessors (GetValue & SetValue)
                do
                {
                    int i = left;
                    int j = right;

                    // pre-sort the low, middle (pivot), and high values in place. 
                    // this improves performance in the face of already sorted data, or 
                    // data that is made up of multiple sorted runs appended together.
                    int middle = GetMedian(i, j);
                    SwapIfGreaterWithItems(i, middle); // swap the low with the mid point
                    SwapIfGreaterWithItems(i, j); // swap the low with the high
                    SwapIfGreaterWithItems(middle, j); // swap the middle with the high

                    Object x = keys.GetValue(middle);
                    do
                    {
                        // Add a try block here to detect IComparers (or their 
                        // underlying IComparables, etc) that are bogus.
                        while (comparer.Compare((float) keys.GetValue(i), (float) x) < 0) i++;
                        while (comparer.Compare((float) x, (float) keys.GetValue(j)) < 0) j--;
                        if (i > j) break;
                        if (i < j)
                        {
                            Object key = keys.GetValue(i);
                            keys.SetValue(keys.GetValue(j), i);
                            keys.SetValue(key, j);
                            if (items != null)
                            {
                                Object item = items.GetValue(i);
                                items.SetValue(items.GetValue(j), i);
                                items.SetValue(item, j);
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
            bool[] result = new bool[concatenated.Length*2]; /*Concatenated float array*/
            for (int i = 0; i < topWavelets; i++)
            {
                int index = indexes[i];
                double value = concatenated[i];
                if (value > 0) /*positive wavelet*/
                    result[index*2] = true;
                else if (value < 0) /*negative wavelet*/
                    result[index*2 + 1] = true;
            }
            return result;
        }

        /// <summary>
        ///   Decode the signature of the fingerprint
        /// </summary>
        /// <param name = "signature">Signature to be decoded</param>
        /// <returns>Array of doubles with positive [10], negatives [01], and zeros [00]</returns>
        public static double[] DecodeFingerprint(bool[] signature)
        {
            int len = signature.Length/2;
            double[] result = new double[len];
            for (int i = 0; i < len*2; i += 2)
            {
                if (signature[i]) // positive if first is true
                    result[i/2] = 1;
                else if (signature[i + 1]) // negative if second is true
                    result[i/2] = -1;
                //otherwise '0'
            }
            return result;
        }

        #endregion

        #region Hashing

        public HashedFingerprint[] GetFingerHashes(IStride stride, List<Fingerprint> listdb)
        {
            List<Fingerprint> listDb = listdb;
            MinHash minHash = new MinHash(true);
            List<byte[]> minhashdb = listDb.Select(fing => minHash.ComputeMinHashSignatureByte(fing.Signature)).ToList();
            var lshBuckets = minhashdb.Select(fing => minHash.GroupMinHashToLSHBucketsByte(fing, 33, 3).Values.ToArray()).ToList();

            //List<HashedFingerprint> hashedFinger = new List<HashedFingerprint>();
            HashedFingerprint[] hashedFinger = new HashedFingerprint[listDb.Count];
            for (int index = 0; index < listDb.Count; index++)
            {
                var hashfinger = new HashedFingerprint(minhashdb[index], lshBuckets[index], listDb[index].SequenceNumber,
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
        /// <param name="tableSize">Number of hashes per array</param>
        /// <returns>
        /// An array of HashedFingerprint objects.
        /// </returns>
        public HashedFingerprint[] GenerateHashedFingerprints(string[] receivedHashes, string[] receivedTimestamps, int tableSize)
        {
            List<long> hashBins = new List<long>();
            List<double> timestamps = new List<double>();

            List<HashedFingerprint> receivedFingerprints = new List<HashedFingerprint>();
            try
            {
                for (int index = 0; index < receivedHashes.Length - 1; index++)
                {
                    hashBins.Add(Convert.ToInt64(receivedHashes[index]));
                    timestamps.Add(Convert.ToDouble(receivedTimestamps[index]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.HResult);
            }

            List<long[]> hashBinsList = new List<long[]>();
            List<double> timestampList = new List<double>();
            for (int j = 0; j < timestamps.Count - 1; j++)
            {
                if (j % tableSize == 0 && hashBins.Count > j + tableSize)
                {
                    long[] bins = new long[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        bins[i] = hashBins[i + j];
                    }
                    hashBinsList.Add(bins);
                    timestampList.Add(timestamps[j]);
                }
            }
            for (var i = 0; i < hashBinsList.Count; i++)
            {
                var finger = new HashedFingerprint(hashBinsList[i], timestampList[i]);
                receivedFingerprints.Add(finger);
            }

            return receivedFingerprints.ToArray();
        }

        #endregion

        #region Recognition
        private readonly List<HashedFingerprint> _matchedFingerprints = new List<HashedFingerprint>();
        private readonly List<int> _bestMatchedFingerprint = new List<int>();
        public bool CompareFingerprintLists(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
        {
            //
            var commonCounter = 0;
            var highestCommon = 0;
            foreach (var fingerprint1 in fingerprints)
            {
                foreach (var fingerprint2 in toCompare)
                {
                    var commonNumbers = from a in fingerprint1.HashBins.AsEnumerable()
                                        join b in fingerprint2.HashBins.AsEnumerable() on a equals b
                                        select a;
                    if (highestCommon < commonNumbers.Count()) highestCommon = commonNumbers.Count();

                    if (commonNumbers.Count() >= 3)
                    {
                        _matchedFingerprints.Add(fingerprint1);
                        // Best matched fingerprint is the fingerprint with the highest number of hashes being equal to original fingerprint
                        _bestMatchedFingerprint.Add(commonNumbers.Count());
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

        public double CompareFingerprintListsHighest(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
        {
            //
            var highestCommon = 0;
            foreach (var fingerprint1 in fingerprints)
            {
                foreach (var fingerprint2 in toCompare)
                {
                    var commonNumbers = from a in fingerprint1.HashBins.AsEnumerable()
                                        join b in fingerprint2.HashBins.AsEnumerable() on a equals b
                                        select a;

                    if (highestCommon < commonNumbers.Count()) highestCommon = commonNumbers.Count();

                    if (commonNumbers.Count() >= 3)
                    {
                        _matchedFingerprints.Add(fingerprint1);
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
                return _matchedFingerprints[_matchedFingerprints.Count - 1].Timestamp;
            }
            // Returns -1 if the lists are not a match
            return -1;
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
