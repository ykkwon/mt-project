using System;

namespace AcousticFingerprintingLibrary_0._4._5.FFT
{
    /// <summary>
    /// Hanning window function
    /// </summary>
    public class HanningWindow
    {
        public double[] GetWindow(int length)
        {
            double[] array = new double[length];
            for (int i = 0; i < length; i++)
                array[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (length - 1)));
            return array;
        }
    }
}