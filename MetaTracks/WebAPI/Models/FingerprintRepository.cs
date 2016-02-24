using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
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
        Dictionary<string, AttributeValue> lastKeyEvaluated = null;
            do
            {
                var request = new ScanRequest
                {
                    TableName = "Video_Fingerprints",
                    Limit = 2,
                    ExclusiveStartKey = lastKeyEvaluated,
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":fingerprint", new AttributeValue { N = "0" }}
                    },
                    FilterExpression = ":fingerprint = Fingerprint",

                    ProjectionExpression = "Fingerprint, Title"
                };

                var response = client.Scan(request);

                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    return item.ToString();
                }
                lastKeyEvaluated = response.LastEvaluatedKey;

            } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);
            return "Test";
        }
    }
}