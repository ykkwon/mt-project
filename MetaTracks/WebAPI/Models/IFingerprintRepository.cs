using System.Collections.Generic;

namespace WebAPI.Models
{
    internal interface IFingerprintRepository
    
    {
        List<string> GetSingleFingerprintByHash(string hash);
        List<string> GetFingerprintsByTitle(string title);
    }
}
