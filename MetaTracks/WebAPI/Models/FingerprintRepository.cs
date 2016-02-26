using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;

namespace WebAPI.Models
{
    public class FingerprintRepository : IFingerprintRepository
    {
        static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        static string tableName = "Video_Fingerprints";
        Table table = Table.LoadTable(client, tableName);
        ScanFilter scanFilter = new ScanFilter();

        public List<string> GetSingleFingerprintByHash(string hash)
        {
            scanFilter.AddCondition("Fingerprint", ScanOperator.Equal, hash);
            Search hashSearch = table.Scan(scanFilter);
            List<Document> fingerprintItems = hashSearch.GetRemaining();
            List<string> stringList = new List<string>();
            if (fingerprintItems.Count != 0)
            {
               stringList.Add(fingerprintItems.Count.ToString());
               return stringList;
            }
            return null;
        }

        public List<string> GetFingerprintsByTitle(string title)
        {
            scanFilter.AddCondition("Title", ScanOperator.Equal, title);
            Search titleSearch = table.Scan(scanFilter);
            List<string> stringList = new List<string>();
            List <Document> titleItems = titleSearch.GetRemaining();
            foreach (Document doc in titleItems)
            {
                string newDoc = doc.ToJson();
                stringList.Add(newDoc);
            }
            if (titleItems.Count != 0)
            {
                return stringList;
            }
            return null;
        }
    }
}