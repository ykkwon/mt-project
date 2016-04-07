using System;

namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides
{
    public class IncrementalStride : Stride
    {
        public IncrementalStride(int incrementBy, int samplesPerFingerprint)
            : base(-samplesPerFingerprint + incrementBy)
            /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
        {
            if (incrementBy <= 0)
            {
                throw new ArgumentException("Bad parameter. IncrementBy should be strictly bigger than 0");
            }
        }

        public IncrementalStride(int incrementBy, int samplesPerFingerprint, int firstStride)
            : this(incrementBy, samplesPerFingerprint)
        {
            FirstStride = firstStride;
        }
    }
}