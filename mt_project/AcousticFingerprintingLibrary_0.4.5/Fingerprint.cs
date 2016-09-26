namespace AcousticFingerprintingLibrary_0._4._5
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
        public HashedFingerprint(long[] hashBins, int sequenceNumber, double sequenceAt)
        {
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            Timestamp = sequenceAt;
        }

        public HashedFingerprint(long[] hashBins, double timeStamp)
        {
            HashBins = hashBins;
            Timestamp = timeStamp;
        }

        public long[] HashBins { get; set; }

        public int SequenceNumber { get; set; }

        public double Timestamp { get; set; }
    }
}
