﻿namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.Wavelets
{
    /// <summary>
    ///   Wavelet decomposition algorithm
    /// </summary>
    public interface IWaveletDecomposition
    {
        /// <summary>
        ///   Apply wavelet decomposition on the selected image
        /// </summary>
        /// <param name = "frames">Frames to be decomposed</param>
        void DecomposeImageInPlace(float[][] frames);
    }
}