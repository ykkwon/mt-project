using System;
using MySql.Data.MySqlClient;

namespace DatabasePopulationApplication_0._4._5
{
    class FingerprintDatabaseManager
    {
        public void confirmConnection()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
           password=Security1;database=system_users";

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
           password=Security1;database=system_users";

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

        public void truncateTable()
        {
            string cs = @"server=webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com;userid=glennskjong;
           password=Security1;database=system_users";

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

