using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;


using System.Data.SQLite;


namespace SUD
{
    public class Dungeon
    {        
        //Variables for Dungeon class
        public Dictionary<Socket, String> socketToRoomLookup;
        SQLiteConnection conn = null;
        string databaseName = "data.database";

        //Function to add various different rooms to the dungeon database
        void AddRoomToDungeon(String roomName, String description, String north, String south, String east, String west)
        {

            String sql = "";
            try
            {
                sql = "insert into " + "table_rooms" + " (name, desc, north, south, east, west) values ";
                sql += "('" + roomName + "'";
                sql += ",'" + description + "'";
                sql += ",'" + north + "'";
                sql += ",'" + south + "'";
                sql += ",'" + east + "'";
                sql += ",'" + west + "'";
                sql += ")";
                

                var command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to add: " + roomName + " : " + description + " to DB " + ex +"\n"+sql);
            }
        }

        //Main function of dungeon class
        public void Init()
        {

            try
            {
                SQLiteConnection.CreateFile(databaseName);

                conn = new SQLiteConnection("Data Source=" + databaseName + ";Version=3;FailIfMissing=True");

                SQLiteCommand command;

                conn.Open();
                //Creating and adding tables to the database
                command = new SQLiteCommand("create table table_rooms (name varchar(20), desc varchar(20), north varchar(20), south varchar(20), east varchar(20), west varchar(20))", conn);
                command.ExecuteNonQuery();

                //command = new SQLiteCommand("drop table table_phonenumbers", conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create DB failed: " + ex);
            }
            //Labels for different rooms to be added to the dungeon
            AddRoomToDungeon("room 0", "room 0 desc","room 1","","","");
            AddRoomToDungeon("room 1", "room 1 desc","","room 0","room 2","");
            AddRoomToDungeon("room 2", "room 2 desc", "room 3", "room 4", "room 1", "");
            AddRoomToDungeon("room 3", "room 3 desc", "room 5", "room 3", "", "");
            AddRoomToDungeon("room 4", "room 4 desc", "room 2", "", "", "");
            AddRoomToDungeon("room 5", "room 5 desc", "room 0", "room 0", "room 0", "room 0");

            try
            {
                Console.WriteLine("");
                SQLiteCommand command = new SQLiteCommand("select * from " + "table_rooms" + " order by name asc", conn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())

                {
                    Console.WriteLine("Name: " + reader["name"] + ": " + reader["desc"]);
                }

                reader.Close();
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to display DB");
            }


            socketToRoomLookup = new Dictionary<Socket, String>();
           
        }
        //Put the player's socket in the current room
        public void SetClientInRoom(Socket client, String room)
        {
            if (socketToRoomLookup.ContainsKey(client) == false)
            {
                socketToRoomLookup[client] = room;
            }
        }
        //Remove the player's socket from the current room
        public void RemoveClient(Socket client)
        {
            if (socketToRoomLookup.ContainsKey(client) == true)
            {
                socketToRoomLookup.Remove(client);
            }
        }
        //Description of the room that the player currently occupies
        public String RoomDescription(Socket player)
        {
            if (socketToRoomLookup.ContainsKey(player) == false)
            {
                return "";
            }

            var oldRoom = socketToRoomLookup[player];

            SQLiteCommand command = new SQLiteCommand("select * from  table_rooms where name == '" + socketToRoomLookup[player] + "'", conn);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.HasRows == true)
            {
                while (reader.Read())
                {
                    var str = reader["desc"].ToString();

                    string[] exits = { "north", "south", "east", "west" };

                    str += "\nExits are:\n";

                    for (var i = 0; i < exits.Length; i++)
                    {
                        if (reader[exits[i]].ToString() != "")
                        {
                            str += exits[i] + " ";
                        }
                    }

                    str += "\n";
                    return str;
                }
            }

            return "";
        }
        public bool UpdatePlayerRoom(Socket player,String direction)
        {
            if (socketToRoomLookup.ContainsKey(player) == false)
            {
                return false;
            }

            var oldRoom = socketToRoomLookup[player];

            SQLiteCommand command = new SQLiteCommand("select * from  table_rooms where name == '" + oldRoom + "'", conn);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.HasRows == true)
            {
                while (reader.Read())
                {
                    Console.WriteLine("Name: " + reader["name"] + reader["desc"]);

                    if(reader[direction].ToString() != "")
                    {
                        socketToRoomLookup[player] = reader[direction].ToString();
                        return true;
                    }                    
                }                
            }
            return false;
        }
    }
}
