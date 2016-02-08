using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace mt_commonlib
{
    internal interface IFingerprint
    {
        void Receive(Movie movie);

        void ReceiveFingerprint(Movie movie);

        void Send(String hash);

        void Preprocess(Movie movie);

        void ComputeSpectrogram(Movie movie);

        void Filter(Array stringArray);

        void ComputeWavelets(Array stringArray);

        void HashTransform(Array waveletArray);

        void PlayFile(Movie movie);
    }
}
