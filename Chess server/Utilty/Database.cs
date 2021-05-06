using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace Chess_server
{
    class Database
    {
        private const string fp = @"../../Db.sql";
        private static SQLiteConnection con;

        // Opens the database file, incase there isn't a database file it creates a new one
        public static void Open()
        {
            if(!File.Exists(fp))
            {
                SQLiteConnection.CreateFile(fp);
                con = new SQLiteConnection("Data Source =" + fp + ";Version=3;");
                con.Open();

                var cmd = new SQLiteCommand(con);

                cmd.CommandText = @"CREATE TABLE USERS(
                                    USERNAME varchar(15) PRIMARY KEY,
                                    PASSWORD varchar(32))";
                cmd.ExecuteNonQuery();
            }
            else
            {
                con = new SQLiteConnection("Data Source =" + fp + ";Version=3;");
                con.Open();
            }
        }

        // Tries to add a user to the database
        // Returns whether or not the action succeeded
        public static bool AddUser(string username, string password)
        {
            try
            {
                var cmd = new SQLiteCommand(con);
                cmd.CommandText = "INSERT INTO USERS(USERNAME, PASSWORD) VALUES('" + username + "','" + password + "')";
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Checks if the password matches the username given by the client
        public static bool CheckPassword(string username, string password)
        {
            var cmd = new SQLiteCommand(con);

            cmd.CommandText = "SELECT USERNAME FROM USERS WHERE USERNAME = '" + username + "' AND PASSWORD = '" + password + "' ";
            if (cmd.ExecuteScalar() != null)
                return true;
            return false;
        }
    }
}
