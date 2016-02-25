using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;

namespace WebAPI.Models
{
    interface IFingerprintRepository
    
    {
        string GetSingleFingerprintByHash(string hash);
        List<string> GetFingerprintsByTitle(string title);
    }
}
