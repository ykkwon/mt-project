﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AcousticFingerprintingLibrary_0._4._5.FFT;
using AcousticFingerprintingLibrary_0._4._5.Hashing;
using AddressBookUI;

namespace AcousticFingerprintingLibrary_0._4._5
{
    public class FingerprintManager
    {
        /// <summary>
        ///   Logarithmic frequency indexes
        /// </summary>
        private readonly int[] _spacedLogFreq;

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
            _lshTableSize = 33;
            _lshKey = 3;
            WindowFunction = new HanningWindow();
            HaarWavelet = new HaarWavelet();
            LogBins = 32;
            FingerprintWidth = 128;
            Overlap = 64; // Spectrogram overlap
            WindowSize = 2048;
            MinFrequency = 318; // Lowest Frequency
            MaxFrequency = 2000; // Highest Frequency
            TopWavelets = 200;
            SampleRate = 8000;
            LogBase = Math.E;
            Stride = -(Overlap*FingerprintWidth) + 1024;
            if (_spacedLogFreq == null)
                _spacedLogFreq = GetLogSpacedFrequencies(MinFrequency, MaxFrequency, WindowSize);
            _windowArray = WindowFunction.GetWindow(WindowSize);
        }

        #region LogFrequencies

        /// <summary>
        ///   Get logarithmically spaced indices
        /// </summary>
        /// <returns>Gets an array of indexes</returns>
        /// <summary>
        ///   Get logarithmically spaced arrays
        /// </summary>
        /// <param name = "minFrequencies">Min frequency</param>
        /// <param name = "maxFrequencies">Max frequency</param>
        private int[] GetLogSpacedFrequencies(int minFrequencies, int maxFrequencies, int fftSize)
        {
            var logMin = Math.Log(minFrequencies, LogBase); // Logn(minFr...);
            var logMax = Math.Log(maxFrequencies, LogBase); // Logn(maxFr...);
            var delta = (logMax - logMin)/LogBins;

            var indexes = new int[LogBins + 1];
            double accDelta = 0;
            for (var index0 = 0; index0 <= LogBins; index0++)
            {
                var freq = (float) Math.Pow(LogBase, logMin + accDelta);
                accDelta += delta;

                var chunk = freq/((float) SampleRate/2);
                indexes[index0] = (int) Math.Round((fftSize/2 + 1)*chunk);
                /*Find the start index in array from which to start the summation*/
            }
            return indexes;
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
        public float[][] CreateImageSpectrogram(string filename, int milliseconds, int startmilliseconds)
        {
            //read 5512 Hz, Mono, PCM, with a specific bassproxy
            var samples = BassLoader.GetSamplesMono(filename, SampleRate);
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
                Fourier.FFT(fourierSignal, windowSize/*, FourierDirection.Forward*/);
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

        public float[][] CreateSpectrogram(float[] samples)
        {
            Console.WriteLine("CreateSpectrogram");
            samples = NormalizeInPlace(samples); // OG DITTA ---------------------------------------------------------------------------------
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
                    fftSamples[2*windowIndex] =
                        (float) (_windowArray[windowIndex]*samples[widthIndex*overlap + windowIndex]);
                    /*Weight by Hann Window*/
                    fftSamples[2*windowIndex + 1] = 0;
                }
                //FFT transform for gathering the spectrogram
                Fourier.FFT(fftSamples, windowSize/*, FourierDirection.Forward*/);
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
        public static float[] NormalizeInPlace(float[] array)
        {
            var samples = array; /// DITTA---------------------------------------------------------------------------------
            double squares = 0;
            var nsamples = samples.Length;
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
            return samples; /// OG DOTTA---------------------------------------------------------------------------------
        }

        #endregion

        #region Frequency Manipulation

        /// <summary>
        ///   Logarithmic spaced frequency bins
        /// </summary>
        /// <param name = "spectrum">Spectrum to space</param>
        public float[] ExtractLogBins(float[] spectrum)
        {
            // Safe Code:
            if (spectrum == null)
                throw new ArgumentNullException(nameof(spectrum));

            var totalFreq = new float[LogBins]; /*32*/
            for (var index = 0; index < LogBins; index++)
            {
                var low = _spacedLogFreq[index];
                var high = _spacedLogFreq[index + 1];

                for (var index2 = low; index2 < high; index2++)
                {
                    double re = spectrum[2 * index2];
                    double img = spectrum[2 * index2 + 1];
                    totalFreq[index] += (float)Math.Sqrt(re * re + img * img);
                }
                totalFreq[index] = totalFreq[index] / (high - low);
            }
            return totalFreq;
        }

        #endregion
        
        #region Fingerprinting

        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "filename">Filename to be analyzed</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(string filename)
        {
            Console.WriteLine("GetSamplesMono");
            var samples = BassLoader.GetSamplesMono(filename, SampleRate);
            return CreateFingerprints(samples);
        }

        /// <summary>
        ///   Create fingerprints from already written samples
        /// </summary>
        /// <param name = "samples">Samples from a media file</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(float[] samples)
        {
            var spectrum = CreateSpectrogram(samples);
            return CreateFingerprints(spectrum);
        }

        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "spectrogram">Spectrogram of the media file</param>
        /// <returns>Fingerprint signatures</returns>
        public List<Fingerprint> CreateFingerprints(float[][] spectrogram)
        {
            Console.WriteLine("CreateFingerprints");
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

                var temp = HaarWavelet.TransformImage(frames); /*Compute wavelets*/
                var image = ExtractTopWavelets(temp);
                //Fingerprint fingerp = new Fingerprint(sequenceNr, image, start * ((double)overlap / sampleRate));
                fingerPrints.Add(new Fingerprint
                {
                    SequenceNumber = sequenceNr++,
                    Signature = image,
                    Timestamp = start*((double) overlap/sampleRate)
                });
                start += fingerprintWidth + Stride/overlap;
            }
            return fingerPrints;
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
            // Safe code:
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));
            for (var j = 0; j < frames.GetLength(0); j++)
                if (frames[j] == null)
                    throw new ArgumentNullException(nameof(frames));
            
            var width = frames.GetLength(0); // 128
            var height = frames[0].Length; // 32

            if (TopWavelets >= width*height || TopWavelets < 0)
                throw new ArgumentException("TopWaveletes cannot exceed the length of array or below 0");

            var concatenated = new float[width * height]; // 128, 32
            var oldConcat = new float[width * height]; // 128, 32
            for (var row = 0; row < width; row++)
            {
                Array.Copy(frames[row], 0, concatenated, row * frames[row].Length, frames[row].Length);
                Array.Copy(frames[row], 0, oldConcat, row * frames[row].Length, frames[row].Length);
            }

            //var indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            AbsClasss abs = new AbsClasss();
            Array.Sort(concatenated, abs);
            //
            var indexes = GetSortedIndexes(concatenated, oldConcat);
            ArraySort(concatenated, indexes);
            
            var result = new bool[concatenated.Length*2]; /*Concatenated float array*/
            for (var i = 0; i < TopWavelets; i++)
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

        public int[] GetSortedIndexes(float[] concat, float[] oldconcat)
        {
            int[] testArray = new int[concat.Length];
            for (int index = 0; index < concat.Length; index++)
            {
                var newf = concat[index];
                for (int index2 = 0; index2 < oldconcat.Length; index2++)
                {
                    var oldf = oldconcat[index2];
                    if (newf == oldf)
                    {
                        testArray[index] = index2;
                    }
                }
            }
            return testArray;
        }

        public class AbsClasss : Comparer<float>
        {
            public override int Compare(float x, float y)
            {
                return Math.Abs(y).CompareTo(Math.Abs(x));
            }
        }

        public static int Compare(float x, float y)
        {
            return Math.Abs(y).CompareTo(Math.Abs(x));
        }
        #endregion

        #region Hashing

        public HashedFingerprint[] GetFingerHashes(List<Fingerprint> fingerprintList)
        {
            int brp = 0;
            var listDb = fingerprintList;
            var minHash = new MinHash();
            var minhashdb = new List<byte[]>();
            for (int i = 0; i < listDb.Count; i++)
            {
                var fing = listDb[i];
                minhashdb.Add(minHash.ComputeMinHashSignatureByte(fing.Signature));
            }
            var lshBuckets = new List<long[]>();

            for (int i = 0; i < minhashdb.Count; i++)
            {
                var fing = minhashdb[i];
                lshBuckets.Add(minHash.GroupMinHashToLshBucketsByte(fing, _lshTableSize, _lshKey).Values.ToArray());
            }

            //List<HashedFingerprint> hashedFinger = new List<HashedFingerprint>();
            var hashedFingerprintList = new HashedFingerprint[listDb.Count];
            for(var index = 0; index < hashedFingerprintList.Length; index++)
            {
                var hashfinger = new HashedFingerprint(lshBuckets[index], listDb[index].SequenceNumber,
                    listDb[index].Timestamp);
                //hashedFinger.Add(hashfinger);
                hashedFingerprintList[index] = hashfinger;
            }

            return hashedFingerprintList;
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
            bool ios = receivedTimestamps[0] != null;

            try
            {
                for (var index = 0; index < receivedHashes.Length - 1; index++)
                {
                    hashBins.Add(Convert.ToInt64(receivedHashes[index]));
                    //timestamps.Add(Convert.ToDouble(receivedTimestamps[index]));
                    if(ios)
                        timestamps.Add(Math.Round(double.Parse(receivedTimestamps[index].Replace(',', '.'))));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.HResult);
            }

            var hashBinsList = new List<long[]>();
            var timestampList = new List<double>();
            for (var j = 0; j < hashBins.Count - 1; j++)
            {
                if (j%_lshTableSize == 0 && hashBins.Count > j + _lshTableSize)
                {
                    var bins = new long[_lshTableSize];
                    for (var i = 0; i < _lshTableSize; i++)
                    {
                        bins[i] = hashBins[i + j];
                    }
                    hashBinsList.Add(bins);
                    if(ios)
                        timestampList.Add(timestamps[j]);
                    else
                        timestampList.Add(0);
                }
            }
            HashedFingerprint[] list = new HashedFingerprint[hashBinsList.Count];
            if (ios)
            {
                for (int i = 0; i < timestampList.Count; i++)
                {
                    var t = hashBinsList[i];
                    HashedFingerprint hash = new HashedFingerprint(t, timestampList[i]);
                    list[i] = hash;
                }
            }
            else
            {
                for (int i = 0; i < hashBinsList.Count; i++)
                {
                    var t = hashBinsList[i];
                    HashedFingerprint hash = new HashedFingerprint(t, timestampList[i]);
                    list[i] = hash;
                }
            }
            
            return list;
            //return hashBinsList.Select((t, i) => new HashedFingerprint(t, timestampList[i])).ToArray();
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
        private bool hasFingerprint = false;
        private bool _needToExpandSearch = false;
        private int _searchFieldSize = 15;
        // Get the newest timeStamp found in recognition
        // Call this to get the newest timestamp found
        public static double LatestTimeStamp { get; set; }

        public double CompareFingerprintLists(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
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
                    var set2 = new HashSet<long>(fingerprint2.HashBins);
                    var i = 0;
                    foreach (var hash in fingerprint1.HashBins)
                    {
                        var qwe = set2.Contains(hash);
                        if (qwe) i++;
                    }
                    var count = i;
                    if (count >= 4)
                    {
                        _matchedFingerprints.Add(fingerprint1);
                        // Best matched fingerprint is the fingerprint with the highest number of hashes being equal to original fingerprint
                        if (highestCommon <= count)
                        {
                            _bestMatchedFingerprint = fingerprint1;
                            hasFingerprint = true;
                            LatestTimeStamp = _bestMatchedFingerprint.Timestamp;
                            highestCommon = count;
                        }
                        // potential match
                        commonCounter++;
                        break; // jumps out of loop and on to next fingerprint
                    }
                }
            }
            // If result is greater than 5% it is a potential match
            var result = (double) (100*commonCounter)/fingerprints.Length;
            return result; // if result greater than 5, return true, else false
        }

        public double CompareFingerprintListsHighest(HashedFingerprint[] fingerprints, HashedFingerprint[] toCompare)
        {
            Console.WriteLine("NEED TO EXPAND? " + _needToExpandSearch);
            Console.WriteLine(_searchFieldSize);

            var fingerprintList = fingerprints;
            var toCompareList = toCompare;
            //
            var commonCounter = 0;
            var highestCommon = 0;

            if (hasFingerprint) // Check if a best fingerprint is found
            {

                bool foundAnyFingerprints = false;
                List<double> timeStamps = new List<double>();
                foreach (var hash in fingerprints)
                    timeStamps.Add(hash.Timestamp);

                var lastTime = LatestTimeStamp;
                List<int> matchingIndexes = new List<int>();

                if (_needToExpandSearch) // If we need to expand search, multiply searchfield by 4
                {
                    _searchFieldSize *= 4;
                }
                var seconds = _searchFieldSize;

                var plusTime = Math.Min(timeStamps.Last(), lastTime + seconds);
                var minusTime = Math.Max(0, lastTime - seconds);
                for (var i = 0; i < timeStamps.Count; i++)
                {
                    if (timeStamps[i] < plusTime && timeStamps[i] >= minusTime)
                        matchingIndexes.Add(i);
                }

                HashedFingerprint[] currentList = new HashedFingerprint[matchingIndexes.Count];
                for (var i = 0; i < matchingIndexes.Count; i++)
                {
                    currentList[i] = fingerprints[matchingIndexes[i]];
                }
                foreach (var list in currentList)
                {
                    foreach (var fingerprint2 in toCompareList)
                    {
                        var set2 = new HashSet<long>(fingerprint2.HashBins);
                        var set3 = fingerprint2.HashBins;

                        /*
                        set3.sort
                        
                        var i = 0;
                        foreach (var hash in list.HashBins)
                        {
                            int qwe = Array.BinarySearch(set3, hash);
                            if (qwe > 0) {
                                i++;    
                            }
                        }
                        */
                        
                        var i = 0;
                        foreach (var hash in list.HashBins)
                        {
                            var qwe = set2.Contains(hash);
                            if (qwe) i++;
                        }
                        var count = i;
                        if (count >= 4)
                        {
                            LatestTimeStamp = list.Timestamp;
                            _matchedFingerprints.Add(list);

                            foundAnyFingerprints = true; // Found a fingerprint
                            _needToExpandSearch = false; // found a fingerprint, so no need to expand searchfield
                            _searchFieldSize = 15; // Reset searchfield to 15 seconds.

                            // Best matched fingerprint is the fingerprint with the highest number of hashes being equal to original fingerprint
                            if (highestCommon <= count)
                            {
                                _bestMatchedFingerprint = list;
                                highestCommon = count;
                            }
                            // potential match
                            commonCounter++;
                            break; // jumps out of loop and on to next fingerprint
                        }
                    }
                }
                if (!foundAnyFingerprints)
                    _needToExpandSearch = true;
            }
            else
            { // Searches entire list if no fingerprint has been found
                foreach (var fingerprint1 in fingerprintList)
                {
                    foreach (var fingerprint2 in toCompareList)
                    {
                        var set2 = new HashSet<long>(fingerprint2.HashBins);
                        var i = 0;
                        foreach (var hash in fingerprint1.HashBins)
                        {
                            var qwe = set2.Contains(hash);
                            if (qwe) i++;
                        }
                        var count = i;
                        if (count >= 4)
                        {
                            _matchedFingerprints.Add(fingerprint1);
                            // Best matched fingerprint is the fingerprint with the highest number of hashes being equal to original fingerprint
                            if (highestCommon <= count)
                            {
                                _bestMatchedFingerprint = fingerprint1;
                                hasFingerprint = true;
                                LatestTimeStamp = _bestMatchedFingerprint.Timestamp;
                                highestCommon = count;
                            }
                            // potential match
                            commonCounter++;
                            break; // jumps out of loop and on to next fingerprint
                        }
                    }
                }
            }


            // If result is greater than 5% it is a potential match
            //var result = (double)(100 * commonCounter) / fingerprints.Length;
            return _matchedFingerprints.Count; // if result greater than 5, return true, else false
        }

        /// <summary>
        /// Searches through all fingerprints from movie to find the section with most correct fingerprints.
        /// This is for faster searching later on
        /// </summary>
        /// <param name="allFingerprints">List returned from of fingerprints in SplitFingerprintLists() method</param>
        /// <param name="toCompare">Microphone recording</param>
        /// <returns>Index of the list with most matched fingerprints.</returns>
        /*public int FindBestFingerprintList(List<HashedFingerprint[]> allFingerprints, HashedFingerprint[] toCompare)
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
        }*/

        #endregion


        #region Sorting code taken from C# .NET sourcecode
        
        private void ArraySort(Array keys, Array items)
        {
            var sorter = new SorterGenericArray(keys, items);
            sorter.QuickSort(0, 0 + keys.Length - 1);
        }


        private static int GetMedian(int low, int hi)
        {
            return low + ((hi - low) >> 1);
        }

        // Private value used by the Sort methods for instances of Array. 
        // This is slower than the one for Object[], since we can't use the JIT helpers 
        // to access the elements.  We must use GetValue & SetValue.
        private struct SorterGenericArray
        {
            private readonly Array _keys;
            private readonly Array _items;

            internal  SorterGenericArray(Array keys, Array items)
            {
                _keys = keys;
                _items = items;
            }

            private void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (Compare((float)_keys.GetValue(a), (float)_keys.GetValue(b)) > 0)
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
                        while (Compare((float) _keys.GetValue(i), (float) x) < 0)
                        {
                            i++;
                        }
                        while (Compare((float) x, (float) _keys.GetValue(j)) < 0)
                        {
                            j--;
                        }
                        if (i > j)
                        {
                            break;
                        }
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
                        if (i != Int32.MaxValue)
                        {
                            ++i;
                        }
                        if (j != Int32.MinValue)
                        {
                            --j;
                        }
                    } while (i <= j);
                    if (j - left <= right - i)
                    {
                        if (left < j)
                        {
                            QuickSort(left, j);
                        }
                        left = i;
                    }
                    else
                    {
                        if (i < right)
                        {
                            QuickSort(i, right);
                        }
                        right = j;
                    }
                } while (left < right);
                int iyiuiuiu = 0;
            }
        }

        #endregion

    }
}
