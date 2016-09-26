using System;
using MySql.Data.MySqlClient;


namespace DatabasePopulationApplication_0._4._5
{
    class FingerprintDatabaseManager
    {
        public void ConfirmConnection()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
           password=Bachelor2016!;database=system_users";

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();
                Console.WriteLine(@"Connected to database. MySQL version : {0}", conn.ServerVersion);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(@"Error: {0}", ex);

            }

            finally
            {
                conn?.Close();
            }
        }

        public void InsertFingerprints(string title, double timestamp, int sequenceNumber, long hash, string tmdbId)
         {
             {
                 string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
            password=Bachelor2016!;database=system_users";

                 MySqlConnection conn = null;

                 try
                 {
                     conn = new MySqlConnection(cs);
                     conn.Open();

                     MySqlCommand cmd = new MySqlCommand();
                     cmd.Connection = conn;
                     cmd.CommandText = "INSERT INTO fingerprintTable VALUES(@id, @title, @timestamp, @sequenceNo, @hash)";
                     cmd.Prepare();

                     cmd.Parameters.AddWithValue("@id", 0);
                     cmd.Parameters.AddWithValue("@title", title);
                     cmd.Parameters.AddWithValue("@timestamp", timestamp);
                     cmd.Parameters.AddWithValue("@sequenceNo", sequenceNumber);
                     cmd.Parameters.AddWithValue("@hash", hash);
                     cmd.Parameters.AddWithValue("@tmdbId", tmdbId);
                     cmd.ExecuteNonQuery();
                     Console.ReadLine();
                 }
                 catch (MySqlException ex)
                 {
                     Console.WriteLine(@"Error: {0}", ex);

                 }

                 finally
                 {
                     if (conn != null)
                     {
                         conn.Close();
                     }
                 }
             }
         }

        public void WriteToMySql(string filepath)
        {


            var newFilePath = filepath.Replace('\\', '/');
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;user=glennskjong;database=system_users;password=Bachelor2016!";
            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(
                             string.Format("LOAD DATA LOCAL INFILE '{0}' INTO TABLE fingerprintTable FIELDS TERMINATED BY ';' LINES TERMINATED BY '\n';", newFilePath), conn))
                {
   
                    cmd.CommandTimeout = 99999;
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine(@"Done");

            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex);

            }

            finally
            {
                conn?.Close();
            }
        }

        public void TruncateTable()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
           password=Bachelor2016!;database=system_users";

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "TRUNCATE fingerprintTable";
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                Console.ReadLine();

            }
            catch (MySqlException ex)
            {
                Console.WriteLine(@"Error: {0}", ex);

            }

            finally
            {
                conn?.Close();
            }
        }
    }
}

