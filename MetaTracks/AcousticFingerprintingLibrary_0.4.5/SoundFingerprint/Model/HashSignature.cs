using System;
using System.Diagnostics;

namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.Model
{
    public class HashSignature
    {
        /// <summary>
        ///   Incremental Id
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Int32 _increment;

        /// <summary>
        ///   Lock object
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        ///   Id
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Int32 _id;

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "soundFile">Hashed SoundFile</param>
        /// <param name = "signature">Signature of the soundFile</param>
        public HashSignature(SoundFile soundFile, int[] signature)
        {
            SoundFile = soundFile;
            Signature = signature;
            lock (LockObject)
            {
                _id = _increment++;
            }
        }

        /// <summary>
        ///   Signature of the soundFile
        /// </summary>
        public int[] Signature { get; private set; }

        /// <summary>
        ///   Track (hashed)
        /// </summary>
        public SoundFile SoundFile { get; private set; }

        /// <summary>
        ///   Id of the hash
        /// </summary>
        public Int32 Id
        {
            get { return _id; }
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}