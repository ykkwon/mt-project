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


        public bool IsValid(string _username, string _password)
        {
            MySqlConnectionStringBuilder connStr = new MySqlConnectionStringBuilder();
            connStr.Server = "webapidb.c7tab1cc7vsa.eu-west-1.rds.amazonaws.com";
            connStr.Port = 3306;
            connStr.Database = "system_users";
            connStr.UserID = "glennskjong";
            connStr.Password = "Security1";
            connStr.CharacterSet = "utf8";

            using (var conn = new MySqlConnection(connStr.ToString()))
            {
                string _query = "SELECT Username FROM System_Users WHERE Username = @u AND Password = @p";

                var cmd = new MySqlCommand(_query, conn);
                cmd.Parameters
                    .Add(new MySqlParameter("@u", MySqlDbType.VarChar))
                    .Value = _username;
                cmd.Parameters
                    .Add(new MySqlParameter("@p", MySqlDbType.VarChar))
                    .Value = Helpers.SHA1.Encode(_password);
                conn.Open();
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
}