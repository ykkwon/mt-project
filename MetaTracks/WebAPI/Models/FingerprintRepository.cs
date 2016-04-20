using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Models
{
    public class FingerprintRepository : IFingerprintRepository
    {
        static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        static string tableName = "Video_Fingerprints";
        Table table = Table.LoadTable(client, tableName);
        ScanFilter scanFilter = new ScanFilter();

        public string GetSingleFingerprintByHash(string hash)
        {
            scanFilter.AddCondition("Fingerprint", ScanOperator.Equal, hash);
            Search hashSearch = table.Scan(scanFilter);
            List<string> stringList = new List<string>();
            List<Document> fingerprintItems = hashSearch.GetRemaining();
            foreach (Document doc in fingerprintItems)
            {
                stringList.Add(doc.ToJson());
            }
            if (fingerprintItems.Count != 0)
            {
                return stringList[0];
            }
            return null;
        }

        public List<string> GetFingerprintsByTitle(string title)
        {
            scanFilter.AddCondition("Title", ScanOperator.Equal, title);
            Search titleSearch = table.Scan(scanFilter);
            List<string> stringList = new List<string>();
            List<Document> titleItems = titleSearch.GetRemaining();
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

        public string GetAllTitlesSQL()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
                password=Bachelor2016!;database=system_users";
            MySqlConnection conn = null;
            conn = new MySqlConnection(cs);
            conn.Open();
            string sql = "SELECT distinct title from fingerprintTable";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            var titles = new StringBuilder();

            while (rdr.Read())
            {
                titles.Append(rdr.GetString("title") + ",");
            }
            return titles.ToString();
        }

        public string GetAllMediaTypesSQL()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
                password=Bachelor2016!;database=system_users";
            MySqlConnection conn = null;
            conn = new MySqlConnection(cs);
            conn.Open();
            string sql = "SELECT distinct title, mediaType from fingerprintTable";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            var mediaTypes = new StringBuilder();

            while (rdr.Read())
            {
                mediaTypes.Append(rdr.GetString("mediaType") + ",");
            }
            return mediaTypes.ToString();
        }

        public string GetAllFingerprintsSQL(string title)
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
                password=Bachelor2016!;database=system_users";
            MySqlConnection conn = null;
            conn = new MySqlConnection(cs);
            conn.Open();
            string sql = string.Format("SELECT hash from fingerprintTable WHERE title =" + "{0}",
                    title);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            var hashes = new StringBuilder();

            while (rdr.Read())
            {
                hashes.Append(rdr.GetString("hash") + ";");
            }
            Console.WriteLine(hashes.ToString());
            return hashes.ToString();
        }

        public string GetAllTimestampsSQL(string title)
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
                password=Bachelor2016!;database=system_users";
            MySqlConnection conn = null;
            conn = new MySqlConnection(cs);
            conn.Open();
            string sql = string.Format("SELECT timestamp from fingerprintTable WHERE title =" + "{0}",
                    title);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            var hashes = new StringBuilder();

            while (rdr.Read())
            {
                hashes.Append(rdr.GetString("timestamp") + ";");
            }
            Console.WriteLine(hashes.ToString());
            return hashes.ToString();
        }
    }
}
