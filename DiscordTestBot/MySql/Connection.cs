using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySqlConnector;


namespace DiscordTestBot.MySql
{
    class Connection
    {
        private MySqlConnection connection = new MySqlConnection("server=db4free.net;port=3306;username=maruf_rav;password=30082002maruf;database=discord_bot_mar");
        
        public bool OpenConnection()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                Console.WriteLine("Connected to MySqlServer");
                return true;
            }
            else
                return false;
        }
        public bool CloseConnection()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
                Console.WriteLine("Disconnected from MySqlServer");
                return true;
            }
            else
                return false;
        }
        public MySqlConnection GetConnection()
        {
            return connection;
        }

        private MySqlCommand CreateSqlCommand(string text)
        {
            MySqlCommand command = GetConnection().CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = text;
            
            command.ExecuteNonQuery();
            return command;
        }
        private MySqlDataReader CreateCommandForReader(string text)
        {
            MySqlCommand command = GetConnection().CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = text;
            MySqlDataReader reader = command.ExecuteReader();
            return reader;
        }
        public int GetAmountOfGold(string nickName)
        {
            CloseConnection();//to update info, should have used more clearly
            OpenConnection();
            int gold = 0;
            MySqlDataReader reader = CreateCommandForReader("select Gold from DiscordBot where NickName = '" + nickName + "'");
            while (reader.Read())
            {
                gold = Convert.ToInt32(reader.GetInt32("Gold"));
            }
            CloseConnection();
            return gold;
        }
        public void AddGold(string nickName, int gold)
        {
            int tempGold = 0;
            tempGold = GetAmountOfGold(nickName);
            OpenConnection();
            gold += tempGold;
            string text = "UPDATE DiscordBot SET Gold = '"+gold+"' WHERE DiscordBot.NickName = '" + nickName + "'";
            MySqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = text;
            command.ExecuteNonQuery();
            CloseConnection();
        }
        public void CreateNewUser(string nickName)
        {
            CloseConnection();
            OpenConnection();
            CreateSqlCommand("INSERT INTO `DiscordBot` (`Id`, `NickName`, `Gold`) VALUES (NULL, '" + nickName + "', '50')");
            CloseConnection();
        }
        public void AssignRole(string nickName)
        {

            int tempGold = 0;
            tempGold = GetAmountOfGold(nickName);
            OpenConnection();
            tempGold = tempGold - 50;
            string text = "UPDATE DiscordBot SET Gold = '" + tempGold + "' WHERE DiscordBot.NickName = '" + nickName + "'";
            MySqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = text;
            command.ExecuteNonQuery();
            CloseConnection();
        }
    }
}
