using System;
using System.Diagnostics;

namespace AcousticFingerprintingLibrary_0._4._5.FFT
{
    /// <summary>
    ///   Static functions for doing various Fourier Operations.
    /// </summary>
    public class Fourier
    {
        private const int cMaxLength = 4096;
        private const int cMinLength = 1;
        private const int cMaxBits = 12;
        private const int cMinBits = 0;
        private static readonly int[][] _reversedBits = new int[cMaxBits][];
        private static int[][] _reverseBits;
        private static int _lookupTabletLength = -1;
        
        private static double[][] _uRLookup;
        private static double[][] _uILookup;
        private static float[][] _uRLookupF;
        private static float[][] _uILookupF;

        private Fourier()
        {
        }

        private static void Swap(ref float a, ref float b)
        {
            float temp = a;
            a = b;
            b = temp;
        }

        private static bool IsPowerOf2(int x)
        {
            return (x & (x - 1)) == 0;
        }

        private static int Pow2(int exponent)
        {
            if (exponent >= 0 && exponent < 31)
            {
                return 1 << exponent;
            }
            return 0;
        }

        private static int Log2(int x)
        {
            if (x <= 65536)
            {
                if (x <= 256)
                {
                    if (x <= 16)
                    {
                        if (x <= 4)
                        {
                            if (x <= 2)
                            {
                                if (x <= 1)
                                {
                                    return 0;
                                }
                                return 1;
                            }
                            return 2;
                        }
                        if (x <= 8)
                            return 3;
                        return 4;
                    }
                    if (x <= 64)
                    {
                        if (x <= 32)
                            return 5;
                        return 6;
                    }
                    if (x <= 128)
                        return 7;
                    return 8;
                }
                if (x <= 4096)
                {
                    if (x <= 1024)
                    {
                        if (x <= 512)
                            return 9;
                        return 10;
                    }
                    if (x <= 2048)
                        return 11;
                    return 12;
                }
                if (x <= 16384)
                {
                    if (x <= 8192)
                        return 13;
                    return 14;
                }
                if (x <= 32768)
                    return 15;
                return 16;
            }
            if (x <= 16777216)
            {
                if (x <= 1048576)
                {
                    if (x <= 262144)
                    {
                        if (x <= 131072)
                            return 17;
                        return 18;
                    }
                    if (x <= 524288)
                        return 19;
                    return 20;
                }
                if (x <= 4194304)
                {
                    if (x <= 2097152)
                        return 21;
                    return 22;
                }
                if (x <= 8388608)
                    return 23;
                return 24;
            }
            if (x <= 268435456)
            {
                if (x <= 67108864)
                {
                    if (x <= 33554432)
                        return 25;
                    return 26;
                }
                if (x <= 134217728)
                    return 27;
                return 28;
            }
            if (x <= 1073741824)
            {
                if (x <= 536870912)
                    return 29;
                return 30;
            }
            return 31;
        }

        private static int[] GetReversedBits(int numberOfBits)
        {
            Debug.Assert(numberOfBits >= cMinBits);
            Debug.Assert(numberOfBits <= cMaxBits);
            if (_reversedBits[numberOfBits - 1] == null)
            {
                int maxBits = Pow2(numberOfBits);
                int[] reversedBits = new int[maxBits];
                for (int i = 0; i < maxBits; i++)
                {
                    int oldBits = i;
                    int newBits = 0;
                    for (int j = 0; j < numberOfBits; j++)
                    {
                        newBits = (newBits << 1) | (oldBits & 1);
                        oldBits = (oldBits >> 1);
                    }
                    reversedBits[i] = newBits;
                }
                _reversedBits[numberOfBits - 1] = reversedBits;
            }
            return _reversedBits[numberOfBits - 1];
        }
        
        private static void ReorderArray(float[] data)
        {
            Debug.Assert(data != null);

            int length = data.Length / 2;

            Debug.Assert(IsPowerOf2(length));
            Debug.Assert(length >= cMinLength);
            Debug.Assert(length <= cMaxLength);

            int[] reversedBits = GetReversedBits(Log2(length));
            for (int i = 0; i < length; i++)
            {
                int swap = reversedBits[i];
                if (swap > i)
                {
                    Swap(ref data[(i << 1)], ref data[(swap << 1)]);
                    Swap(ref data[(i << 1) + 1], ref data[(swap << 1) + 1]);
                }
            }
        }

        private static int _ReverseBits(int bits, int n)
        {
            int bitsReversed = 0;
            for (int i = 0; i < n; i++)
            {
                bitsReversed = (bitsReversed << 1) | (bits & 1);
                bits = (bits >> 1);
            }
            return bitsReversed;
        }

        private static void InitializeReverseBits(int levels)
        {
            _reverseBits = new int[levels + 1][];
            for (int j = 0; j < (levels + 1); j++)
            {
                int count = (int)Math.Pow(2, j);
                _reverseBits[j] = new int[count];
                for (int i = 0; i < count; i++)
                {
                    _reverseBits[j][i] = _ReverseBits(i, j);
                }
            }
        }

        private static void SyncLookupTableLength(int length)
        {
            Debug.Assert(length < 1024 * 10);
            Debug.Assert(length >= 0);
            if (length > _lookupTabletLength)
            {
                int level = (int)Math.Ceiling(Math.Log(length, 2));
                InitializeReverseBits(level);
                InitializeComplexRotations(level);
                _lookupTabletLength = length;
            }
        }

        private static void InitializeComplexRotations(int levels)
        {
            int ln = levels;

            _uRLookup = new double[levels + 1][];
            _uILookup = new double[levels + 1][];

            _uRLookupF = new float[levels + 1][];
            _uILookupF = new float[levels + 1][];

            int N = 1;
            for (int level = 1; level <= ln; level++)
            {
                int M = N;
                N <<= 1;
                {
                    double uR = 1;
                    double uI = 0;
                    double angle = Math.PI / M * 1;
                    double wR = Math.Cos(angle);
                    double wI = Math.Sin(angle);

                    _uRLookup[level] = new double[M];
                    _uILookup[level] = new double[M];
                    _uRLookupF[level] = new float[M];
                    _uILookupF[level] = new float[M];

                    for (int j = 0; j < M; j++)
                    {
                        _uRLookupF[level][j] = (float)(_uRLookup[level][j] = uR);
                        _uILookupF[level][j] = (float)(_uILookup[level][j] = uI);
                        double uwI = uR * wI + uI * wR;
                        uR = uR * wR - uI * wI;
                        uI = uwI;
                    }
                }
            }
        }

        public static void FFT(float[] data, int length, FourierDirection direction)
        {
            Debug.Assert(data != null);
            Debug.Assert(data.Length >= length * 2);
            Debug.Assert(IsPowerOf2(length));

            SyncLookupTableLength(length);

            int ln = Log2(length);

            // reorder array
            ReorderArray(data);

            // successive doubling
            int N = 1;
            int signIndex;
            if (direction == FourierDirection.Forward) signIndex = 0;
            else signIndex = 1;

            for (int level = 1; level <= ln; level++)
            {
                int M = N;
                N <<= 1;

                float[] uRLookup = _uRLookupF[level];
                float[] uILookup = _uILookupF[level];

                for (int j = 0; j < M; j++)
                {
                    float uR = uRLookup[j];
                    float uI = uILookup[j];

                    for (int evenT = j; evenT < length; evenT += N)
                    {
                        int even = evenT << 1;
                        int odd = (evenT + M) << 1;

                        float r = data[odd];
                        float i = data[odd + 1];

                        float odduR = r * uR - i * uI;
                        float odduI = r * uI + i * uR;

                        r = data[even];
                        i = data[even + 1];

                        data[even] = r + odduR;
                        data[even + 1] = i + odduI;

                        data[odd] = r - odduR;
                        data[odd + 1] = i - odduI;
                    }
                }
            }
        }
    }
}