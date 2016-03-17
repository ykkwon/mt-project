using System;
using AcousticFingerprintingLibrary;
using AcousticFingerprintingLibrary.SoundFingerprint;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies;
using AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies.Strides;

namespace iOSApplication
{
    public static class Record
    {
        public static void WriteToForeground()
        {
            FingerprintManager fm = new FingerprintManager();
            Console.WriteLine("TEST");
        }

        public static void getInput()
        {
            throw new NotImplementedException();
        }
    }
}
