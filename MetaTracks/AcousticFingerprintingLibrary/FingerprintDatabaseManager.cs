using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace AcousticFingerprintingLibrary
{
    class FingerprintDatabaseManager : IFingerprintDatabaseManager
    {
        private static readonly AmazonDynamoDBClient Client = new AmazonDynamoDBClient();
        private static string tableName = "Video_Fingerprints";

        public void SendFullFileToDatabase()
        {
            throw new NotImplementedException();
         /**
            Table table = Table.LoadTable(Client, tableName);
            var input = new Document();
            int i = 1;
            DateTime now = DateTime.Now;
            
            foreach (string chunks in HashedChunks)
            {
                Console.WriteLine("Hash " + i + " of " + HashedChunks.Count);
                input["Fingerprint"] = chunks;
                input["Timestamp"] = i++;
                input["Title"] = entryName;
                input["Type"] = "N/A";


                DocumentBatchWrite dbw = new DocumentBatchWrite(table);
                dbw.AddDocumentToPut(input);
                dbw.Execute();
            }
            DateTime then = DateTime.Now;
            Console.WriteLine("Finished at: " + then);
            MainWindow.Main.Status = "Movie has been added to database successfully.";
            MainWindow.Main.Status = "Finished at: " + then;
            MainWindow.Main.Status = "Elapsed: " + (then.Second - now.Second) + " seconds.";
        **/
        }

        public void CompareHashToDatabase()
        {
            throw new System.NotImplementedException();
        }

        public void CreateTestTable()
        {

            // Attribute definitions
            var attributeDefinitions = new List<AttributeDefinition>()
            {
                {new AttributeDefinition {AttributeName = "Fingerprint", AttributeType = "S"}},
                {new AttributeDefinition {AttributeName = "Timestamp", AttributeType = "N"}},
                {new AttributeDefinition {AttributeName = "Title", AttributeType = "S"}},
                {new AttributeDefinition {AttributeName = "Type", AttributeType = "S"}}
            };

            // Key schema for table
            var tableKeySchema = new List<KeySchemaElement>() {
                {
                    new KeySchemaElement {
                        AttributeName= "Fingerprint",
                        KeyType = "HASH"  //Partition key
                    }
                },
                {
                    new KeySchemaElement {
                        AttributeName = "Timestamp",
                        KeyType = "RANGE"  //Sort key
                    }
                }
            };

            // Initial provisioned throughput settings for the indexes
            var ptIndex = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5,
            };

            // CreateDateIndex
            var createDateIndex = new GlobalSecondaryIndex()
            {
                IndexName = "FingerprintIndex",
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                    new KeySchemaElement {
                        AttributeName = "Fingerprint", KeyType = "HASH"  //Partition key
                    },
                    new KeySchemaElement {
                        AttributeName = "Timestamp", KeyType = "RANGE"  //Sort key
                    }
                },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };

            // TitleIndex
            var titleIndex = new GlobalSecondaryIndex()
            {
                IndexName = "TitleIndex",
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                    new KeySchemaElement {
                        AttributeName = "Title", KeyType = "HASH"  //Partition key
                    },
                    new KeySchemaElement {
                        AttributeName = "Type", KeyType = "RANGE"  //Sort key
                    }
                },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };

            var timestampTitleIndex = new GlobalSecondaryIndex()
            {
                IndexName = "Timestamp-Title-index",
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                    new KeySchemaElement {
                        AttributeName = "Timestamp", KeyType = "HASH"  //Partition key
                    },
                    new KeySchemaElement {
                        AttributeName = "Title", KeyType = "RANGE"  //Sort key
                    }
                },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };



            var createTableRequest = new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                },
                AttributeDefinitions = attributeDefinitions,
                KeySchema = tableKeySchema,
                GlobalSecondaryIndexes = { createDateIndex, titleIndex, timestampTitleIndex }
            };
            
            Client.CreateTable(createTableRequest);
            //WaitUntilTableReady(tableName);
            

        }

        public void WaitUntilTableReady(string TableName)
        {
            throw new NotImplementedException();
            /**
            string status = null;
            // Let us wait until table is created. Call DescribeTable.
            do
            {
                Thread.Sleep(5000);
                try
                {
                    var res = Client.DescribeTable(new DescribeTableRequest
                    {
                        TableName = dbTableName
                    });
                    
                    Console.WriteLine("Table name: {0}, status: {1}",
                                   res.Table.TableName,
                                   res.Table.TableStatus);
                    status = res.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                    // DescribeTable is eventually consistent. So you might
                    // get resource not found. So we handle the potential exception.
                }
            } while (status != "ACTIVE");
        **/    
    }
        
        public void RemoveTestTable()
        { 
            var request = new DeleteTableRequest
            {
                TableName = tableName
            };

            Client.DeleteTable(request);
        }
    }
}
