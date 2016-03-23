using AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.FFT;

namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.Windows
{
    public interface IWindowFunction
    {
        /// <summary>
        ///   Window the outer space in place
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "windowLength">Window length</param>
        void WindowInPlace(float[] outerspace, int windowLength);

        /// <summary>
        ///   Window the outer space in place
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "windowLength">Window length</param>
        void WindowInPlace(Complex[] outerspace, int windowLength);

        /// <summary>
        ///   Gets the corresponding window function
        /// </summary>
        /// <param name = "length">Length of the window</param>
        /// <returns>Window function</returns>
        double[] GetWindow(int length);
    }
}