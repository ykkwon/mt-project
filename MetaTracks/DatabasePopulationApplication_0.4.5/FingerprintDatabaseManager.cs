using System;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace DatabasePopulationApplication_0._4._5
{
    class FingerprintDatabaseManager
    {
        public void confirmConnection()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
           password=Bachelor2016!;database=system_users";

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();
                Console.WriteLine("Connected to database. MySQL version : {0}", conn.ServerVersion);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());

            }

            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public void insertFingerprints(string title, double timestamp, int sequenceNumber, long hash)
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
                     cmd.ExecuteNonQuery();
                     Console.ReadLine();
                 }
                 catch (MySqlException ex)
                 {
                     Console.WriteLine("Error: {0}", ex.ToString());

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

        /*public void writeToMySQL(string filepath)
        {
            string connStr = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;user=glennskjong;database=system_users;password=Bachelor2016!;";
            MySqlConnection conn = new MySqlConnection(connStr);

            MySqlBulkLoader bl = new MySqlBulkLoader(conn);
            bl.TableName = "fingerprintTable";
            bl.FieldTerminator = ",";
           // bl.LineTerminator = "\n";
            bl.FileName = filepath;
            bl.NumberOfLinesToSkip = 0;

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Upload data from file
                int count = bl.Load();
                Console.WriteLine(count + " lines uploaded.");

                string sql = "SELECT id, title, timestamp, sequenceNo, hash FROM fingerprintTable";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

               /* while (rdr.Read())
                {
                    Console.WriteLine(rdr[0] + " -- " + rdr[1] + " -- " + rdr[2]);
                }*/

          /*      rdr.Close();

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Done.");
        }*/

        public void writeToMySQL(string filepath)
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;user=glennskjong;database=system_users;password=Bachelor2016!;";

            using (MySqlConnection sqlCon = new MySqlConnection(cs))
            {
                try
                {
                    sqlCon.Open();
                    MySqlCommand sqlCmd =
                        new MySqlCommand("LOAD DATA INFILE 'filepath' INTO TABLE fingerprintTable FIELDS TERMINATED BY ',';", sqlCon);
                    sqlCmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Error: {0}", ex.ToString());
                }
            }
        }

        public void truncateTable()
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
                Console.WriteLine("Error: {0}", ex.ToString());

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
}

