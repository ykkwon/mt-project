using NAudio.Wave;

namespace dbApp.Fingerprint
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

