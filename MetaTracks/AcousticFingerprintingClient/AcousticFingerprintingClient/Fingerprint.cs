using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcousticFingerprintingClient
{
    class Fingerprint
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Hash { get; set; }
        public string Type { get; set; }
    }
}
