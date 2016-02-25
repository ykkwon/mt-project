using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;

namespace WebAPI.Models
{
    interface IFingerprintRepository
    
    {
        string GetFingerprintByHash(string hash);
    }
}
