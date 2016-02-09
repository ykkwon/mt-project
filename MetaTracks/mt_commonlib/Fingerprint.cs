using NAudio.Wave;

namespace mt_commonlib
{
    class Fingerprint
    {
        public WaveFormat Wave { get; set; }

        public Fingerprint(WaveFormat Wave)
        {
            this.Wave = Wave;
        }
    }
}

