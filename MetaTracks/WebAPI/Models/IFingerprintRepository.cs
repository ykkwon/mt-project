using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    interface IFingerprintRepository
    
    {
        IEnumerable<Fingerprint> GetAll();
        Fingerprint Get(int id);
        Fingerprint Add(Fingerprint item);
        bool Update(Fingerprint item);
    }
}
