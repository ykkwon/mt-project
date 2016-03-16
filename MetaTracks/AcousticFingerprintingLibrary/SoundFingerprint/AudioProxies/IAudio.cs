﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcousticFingerprintingLibrary.SoundFingerprint.AudioProxies
{
    public interface IAudio : IDisposable
    {
        /// <summary>
        ///   Read from file at a specific frequency rate
        /// </summary>
        /// <param name = "filename">Filename to read from</param>
        /// <param name = "samplerate">Sample rate</param>
        /// <param name = "milliseconds">Milliseconds to read</param>
        /// <param name = "startmilliseconds">Start at a specific millisecond range</param>
        /// <returns>Array with data</returns>
        float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmilliseconds);
    }
}
