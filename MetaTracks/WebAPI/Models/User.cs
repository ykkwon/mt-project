using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class User
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember on this computer")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Checks if user with given password exists in the database
        /// </summary>
        /// <param name="_username">User name</param>
        /// <param name="_password">User password</param>
        /// <returns>True if user exist and password is correct</returns>
        public bool IsValid(string _username, string _password)
        {
            string connStr = "server=webapidb.cv2ggww0l9ib.us-;user=glennskjong;database=system_users;port=3306;password=Security1;";
            using (var conn = new MySqlConnection(connStr))
            {
                
                string _mysql = "SELECT Username FROM System_Users WHERE Username = @u AND Password = @p";
                try
                {
                    Console.WriteLine("Connecting to MySQL...");

                    var cmd = new MySqlCommand(_mysql, conn);
                    cmd.Parameters
                        .Add(new MySqlParameter("@u", MySqlDbType.VarChar))
                        .Value = _username;
                    cmd.Parameters
                        .Add(new MySqlParameter("@p", MySqlDbType.VarChar))
                        .Value = _password;
                    conn.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Dispose();
                        cmd.Dispose();
                        conn.Close();
                        return true;
                    }
                    else
                    {
                        reader.Dispose();
                        cmd.Dispose();
                        conn.Close();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return false;
                Console.WriteLine("Done.");
            }
        }
    }
}
        /**
        public bool IsValid(string _username, string _password)
        {
            using (var cn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\cfmcdb.mdf;Integrated Security=True"))

            {
                string _sql = @"SELECT [Username] FROM [dbo].[System_Users] WHERE [Username] = @u AND [Password] = @p";
                var cmd = new SqlCommand(_sql, cn);
                cmd.Parameters
                    .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                    .Value = _username;
                cmd.Parameters
                    .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                    .Value = Helpers.SHA1.Encode(_password);
                cn.Open();
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Dispose();
                    cmd.Dispose();
                    return true;
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                    return false;
                }
            }
        }
    }
    **/
