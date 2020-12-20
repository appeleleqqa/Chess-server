﻿using System;
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
        private static SQLiteConnection con;

        // Opens the database file, incase there isn't a database file it creates a new one
        public static void Open()
        {
            if(!File.Exists(@"../../Db.SQL"))
            {
                SQLiteConnection.CreateFile(@"../../Db.SQL");
                con = new SQLiteConnection("Data Source =../../Db.SQL;Version=3;");
                con.Open();

                var cmd = new SQLiteCommand(con);

                cmd.CommandText = @"CREATE TABLE USERS(
                                    USERNAME varchar(15) PRIMARY KEY,
                                    PASSWORD varchar(32))";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE REMEMBER(
                                    IP varchar(15) PRIMARY KEY,
                                    USERNAME varchar(15))";
                cmd.ExecuteNonQuery();
            }
            else
            {
                con = new SQLiteConnection("Data Source =../../Db.SQL;Version=3;");
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
            catch(Exception e)
            {
                return false;
            }
        }

        // Checks if the password matches the username given by the client
        public static bool CheckPassword(string username, string password)
        {
            var cmd = new SQLiteCommand(con);

            cmd.CommandText = "SELECT USERNAME FROM USERES WHERE USERNAME = '" + username + "' AND PASSWORD = '" + password + "' ";
            string user = cmd.ExecuteScalar().ToString();
            if (user != null)
                return true;
            return false;
        }

        // Checks if the user is remembered by the server
        // Returns the username if it is
        // Returns null if its not
        public static string CheckIp(string ip)
        {
            var cmd = new SQLiteCommand(con);

            cmd.CommandText = "SELECT USERNAME FROM REMEMBER WHERE IP = '" +ip + "'";
            return cmd.ExecuteScalar().ToString();
        }
    }
}