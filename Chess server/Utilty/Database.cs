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
        private const string filePath = @"../../Db.sql";
        private static SQLiteConnection con;

        /// <summary>
        /// Opens the database file, in the case where there isn't one it creates a new one
        /// </summary>
        public static void Open()
        {
            if(!File.Exists(filePath))
            {
                SQLiteConnection.CreateFile(filePath);
                con = new SQLiteConnection("Data Source =" + filePath + ";Version=3;");
                con.Open();

                var cmd = new SQLiteCommand(con);

                cmd.CommandText = @"CREATE TABLE USERS(
                                    USERNAME varchar(15) PRIMARY KEY,
                                    PASSWORD varchar(32))";
                cmd.ExecuteNonQuery();
            }
            else
            {
                con = new SQLiteConnection("Data Source =" + filePath + ";Version=3;");
                con.Open();
            }
        }

        /// <summary>
        /// tries to add a new user to the database
        /// </summary>
        /// <param name="username">the player's username</param>
        /// <param name="password">the player's password</param>
        /// <returns>if the action was successful or not</returns>
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

        /// <summary>
        /// Checks if the password matches the username given by the client
        /// </summary>
        /// <param name="username">the player's username</param>
        /// <param name="password">the player's password</param>
        /// <returns>if the action was successful or not</returns>
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
