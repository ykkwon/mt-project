using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Collections;
using System.Collections.Generic;

namespace WebAPI.Models
{
    public class FingerprintRepository : IFingerprintRepository
    {

        public FingerprintRepository()
        {

        }

        public string GetFingerprintByHash(string hash)
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            var tableName = "Video_Fingerprints";
            Table table = Table.LoadTable(client, tableName);

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Fingerprint", ScanOperator.Equal, hash);
            Search ageSearch = table.Scan(scanFilter);
            List<Document> fingerprintItems = ageSearch.GetRemaining();
            if (fingerprintItems.Count != 0)
            {
                return "Found a match.";
            }
            else
            {
                return "No match.";
            }
        }
    }
}