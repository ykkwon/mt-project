using System;

namespace AcousticFingerprintingLibrary_0._4._5.Hashing
{
    /// <summary>
    ///   Haar wavelet transformation algorithm
    /// </summary>
    public class HaarWavelet
    {
        /// <summary>
        ///   Apply Haar Wavelet transformation on the 2d array
        /// </summary>
        /// <param name = "array">Array being transformed</param>
        public void TransformImage(float[][] array)
        {
            Transform(array);
        }

        /// <summary>
        ///   Transformation taken from
        ///   Wavelets for Computer Graphics: A Primer Part by Eric J. Stollnitz Tony D. DeRose David H. Salesin
        /// </summary>
        /// <param name = "array">Array to be decomposed</param>
        private static void Transform(float[] array)
        {
            var haar = array.Length;
            for (var index0 = 0; index0 < haar; index0++) /*doesn't introduce any change in the final fingerprint array*/
                array[index0] /= (float)Math.Sqrt(haar); /*because works as a linear normalize*/
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
        }

        /// <summary>
        /// The two dimensional haar wavelet transform. It uses the one-dimensional transform for each row,
        /// then a one-dimensional transform for each column
        /// </summary>
        /// <param name = "array"> Array to be transformed</param>
        private static void Transform(float[][] array)
        {
            var rowWidth = array.GetLength(0);
            var columnHeight = array[0].Length; 

            for (var row = 0; row < rowWidth; row++) // Transformation of each row
                Transform(array[row]);

            for (var col = 0; col < columnHeight; col++) // Transformation of each column
            {
                var column = new float[rowWidth]; /*Length of each column is equal to number of rows*/
                for (var row = 0; row < rowWidth; row++)
                    column[row] = array[row][col]; // Saves values into temp array "column"

                Transform(column); // 1d Transforms column

                for (var row = 0; row < rowWidth; row++)
                    array[row][col] = column[row]; // Saves results into array
            }
        }
    }
}