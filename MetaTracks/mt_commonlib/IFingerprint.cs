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
        void Receive(Object file);

        void ReceiveFingerprint(Object file);

        void Send(String hash);

        void Preprocess(Object file);

        void ComputeSpectrogram(Object file);

        void Filter(Array stringArray);

        void ComputeWavelets(Array stringArray);

        void HashTransform(Array waveletArray);
    }
}
