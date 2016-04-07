using System;

namespace AcousticFingerprintingLibrary_0._4._5.DistanceClasses
{
    public class IncrementalDistance : Distance
    {
        public IncrementalDistance(int incrementBy, int samplesPerFingerprint)
            : base(-samplesPerFingerprint + incrementBy)
            /*Negative Distance will guarantee that the signal is incremented by the parameter specified*/
        {
            if (incrementBy <= 0)
            {
                throw new ArgumentException("Bad parameter. IncrementBy should be strictly bigger than 0");
            }
        }
    }
}