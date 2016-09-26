using System;

namespace AcousticFingerprintingLibrary_0._4._5.Hashing
{
    /// <summary>
    ///  Haar wavelet transformation algorithm
    /// </summary>
    public class HaarWavelet
    {
        private float[][] globalArrays;
        /// <summary>
        ///   Apply Haar Wavelet transformation on the 2d array
        /// </summary>
        /// <param name = "array">Array being transformed</param>
        public float[][] TransformImage(float[][] array)
        {
            globalArrays = array; // Use a global variable to avoid using pointers
            TwoHaarWavelet();
            return globalArrays;
        }

        public void TwoHaarWavelet()
        {
            var rowWidth = globalArrays.GetLength(0);
            var columnHeight = globalArrays[0].Length;

            for (var row = 0; row < rowWidth; row++) // Transformation of each row
                globalArrays[row] = OneHaarWavelet(globalArrays[row]);

            for (var col = 0; col < columnHeight; col++) // Transformation of each column
            {
                var column = new float[rowWidth]; /*Length of each column is equal to number of rows*/
                for (var row = 0; row < rowWidth; row++)
                    column[row] = globalArrays[row][col]; // Saves values into temp array "column"

                var colm = OneHaarWavelet(column); // 1d Transforms column

                for (var row = 0; row < rowWidth; row++)
                    globalArrays[row][col] = colm[row]; // Saves results into array
            }
        }

        public float[] OneHaarWavelet(float[] arrayNN)
        {
            var array = arrayNN; // Avoid pointers
            var haar = array.Length;
            for (var index0 = 0; index0 < haar; index0++)
                array[index0] /= (float)Math.Sqrt(haar);
            var temp = new float[haar];

            while (haar > 1)
            {
                haar /= 2;
                for (var index0 = 0; index0 < haar; index0++)
                {
                    temp[index0] = (float)((array[2 * index0] + array[2 * index0 + 1]) / Math.Sqrt(2));
                    temp[haar + index0] = (float)((array[2 * index0] - array[2 * index0 + 1]) / Math.Sqrt(2));
                }
                for (var index = 0; index < 2 * haar; index++)
                {
                    array[index] = temp[index];
                }
            }
            return array;
        }
    }
}