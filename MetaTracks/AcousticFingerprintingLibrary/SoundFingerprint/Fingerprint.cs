using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcousticFingerprintingLibrary.SoundFingerprint
{
    public class Fingerprint
    {
        public bool[] Signature { get; set; }

        // Number of fingerprint in file
        public int SequenceNumber { get; set; }

        // Where in soundfile fingerprint is
        public double Timestamp { get; set; }
        
    }

    public class HashedFingerprint
    {
        public HashedFingerprint(byte[] subFingerprint, long[] hashBins, int sequenceNumber, double sequenceAt)
        {
            SubFingerprint = subFingerprint;
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            Timestamp = sequenceAt;
        }

        public byte[] SubFingerprint { get; set; }

        public long[] HashBins { get; set; }

        public int SequenceNumber { get; set; }

        public double Timestamp { get; set; }
    }
}
