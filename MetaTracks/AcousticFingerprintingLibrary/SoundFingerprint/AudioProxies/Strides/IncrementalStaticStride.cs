using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies.Strides
{
    using System;

    public class IncrementalStaticStride : StaticStride
    {
        public IncrementalStaticStride(int incrementBy, int samplesPerFingerprint)
            : base(-samplesPerFingerprint + incrementBy)
            /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
        {
            if (incrementBy <= 0)
            {
                throw new ArgumentException("Bad parameter. IncrementBy should be strictly bigger than 0");
            }
        }

        public IncrementalStaticStride(int incrementBy, int samplesPerFingerprint, int firstStride)
            : this(incrementBy, samplesPerFingerprint)
        {
            FirstStride = firstStride;
        }
    }
}