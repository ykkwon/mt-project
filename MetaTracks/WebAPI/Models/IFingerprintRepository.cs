using System.Collections.Generic;

namespace WebAPI.Models
{
    internal interface IFingerprintRepository
    
    {
        string GetSingleFingerprintByHash(string hash);
        List<string> GetFingerprintsByTitle(string title);
        string GetAllTitlesSQL();
        string GetAllFingerprintsSQL(string title);
        string GetAllTimestampsSQL(string title);
    }
}
