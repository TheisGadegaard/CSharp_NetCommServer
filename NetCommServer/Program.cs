using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCommServer
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Chatroom> chatrooms = new List<Chatroom>();

            NetComm.Host Server;

            Server = new NetComm.Host(3000);
            Server.StartConnection();
            Console.WriteLine("Connection started...");
            Server.onConnection += new NetComm.Host.onConnectionEventHandler(Server_onConnection);
            Console.WriteLine("onConnectionEventHandler setup...");
            Server.lostConnection += new NetComm.Host.lostConnectionEventHandler(Server_lostConnection);
            Console.WriteLine("lostConnectionEventHandler setup...");
            Server.DataReceived += new NetComm.Host.DataReceivedEventHandler(Server_DataReceived);
            Console.WriteLine("DataReceivedEventHandler setup...");
            Console.WriteLine("Server ready");

            void Server_onConnection(string id)
            {
                Console.WriteLine(id + " Connected");
                Console.Write("Users connected: ");
                for (int i = 0; i < Server.Users.Count(); i++)
                {
                    Console.Write(Server.Users.ElementAt(i));

                    if (i + 1 != Server.Users.Count())
                    {
                        Console.Write(", ");
                    }
                }
                Console.WriteLine();

                sendChatroomUpdate(id);

            }
            void Server_lostConnection(string id)
            {
                Console.WriteLine(id + " Disconnected");

                foreach (Chatroom chatroom in chatrooms)
                {
                    chatroom.removeUser(id);
                }

                ClearEmptyRooms();
                printChatrooms();
                sendChatroomUpdateAll();

            }

            void ClearEmptyRooms()
            {
                Boolean chatroomCountChanged = false;
                List<Chatroom> tempChatrooms = new List<Chatroom>();
                foreach (Chatroom chatroom in chatrooms)
                {
                    if (chatroom.getUserList().Count != 0)
                    {
                        tempChatrooms.Add(chatroom);
                    }
                }
                if (chatrooms.Count != tempChatrooms.Count)
                {
                    chatroomCountChanged = true;
                }
                chatrooms = tempChatrooms;
                if (chatroomCountChanged)
                {
                    sendChatroomUpdateAll();
                }
            }

            void sendChatroomUpdate(string id)
            {
                // Console.WriteLine("sendChatroomUpdate() called by id=" + id);
                string message = "CHATROOMS";
                for (int i = 0; i < chatrooms.Count; i++)
                {
                    message += " " + chatrooms.ElementAt(i).getRoomName();
                    message += " " + chatrooms.ElementAt(i).getUserList().Count;
                }

                Console.WriteLine("message= " + message);
                Server.SendData(id, ConvertStringToBytes(message));
            }

            void sendChatroomUpdateAll()
            {
                // Console.WriteLine("sendChatroomUpdateAll called");
                string message = "CHATROOMS";
                for (int i = 0; i < chatrooms.Count; i++)
                {
                    message += " " + chatrooms.ElementAt(i).getRoomName();
                    message += " " + chatrooms.ElementAt(i).getUserList().Count;
                }

                Console.WriteLine("message= " + message);

                for (int j = 0; j < Server.Users.Count; j++)
                {
                    Server.SendData(Server.Users.ElementAt(j), ConvertStringToBytes(message));
                }
            }

            void printChatrooms()
            {
                Console.WriteLine("Number of chatrooms = " + chatrooms.Count);
                Console.Write("chatrooms: ");
                for (int i = 0; i < chatrooms.Count; i++)
                {
                    Console.Write(chatrooms.ElementAt(i).getRoomName() + "(");
                    for(int j = 0; j < chatrooms.ElementAt(i).getUserList().Count; j++)
                    {
                        Console.Write(chatrooms.ElementAt(i).getUserList().ElementAt(j));
                        if(j + 1 != chatrooms.ElementAt(i).getUserList().Count)
                        {
                            Console.Write(", ");
                        }
                    }
                    Console.Write(")");

                    if (i + 1 != chatrooms.Count)
                    {
                        Console.Write(", ");
                    }
                }
                Console.WriteLine();
            }

            void Server_DataReceived(string ID, byte[] Data)
            {
                Console.WriteLine(ID + ": " + ConvertBytesToString(Data));

                string[] splitData = ConvertBytesToString(Data).Split(new string[] { " " }, 2, StringSplitOptions.None);  //Splits the first word of the data from the rest in order to seperate commands from data

                switch (splitData[0])
                {
                    case "NEW_CHATROOM":            //A request to make a new chatroom should look like this: "NEW_CHATROOM *name*"

                        Boolean chatRoomExists = false;
                        for (int i = 0; i < chatrooms.Count; i++)
                        {
                            if (chatrooms.ElementAt(i).Equals(splitData[1]))
                            {
                                chatRoomExists = true;
                            }
                        }

                        if (!chatRoomExists)
                        {
                            chatrooms.Add(new Chatroom(splitData[1]));
                            Console.WriteLine(ID + " created a chatroom named " + splitData[1]);
                            Server.SendData(ID, ConvertStringToBytes("NEW_CHATROOM_CONFIRMED " /*+ splitData[1]*/));
                        }

                        for (int i = 0; i < chatrooms.Count; i++)
                        {
                            if (chatrooms.ElementAt(i).getRoomName().Equals(splitData[1]))
                            {
                                chatrooms.ElementAt(i).addUser(ID);
                            }
                        }
                        sendChatroomUpdateAll();
                        printChatrooms();
                        break;

                    case "JOIN_CHATROOM":           //A request to join a chatroom should look like this: "JOIN_CHATROOM *name*"
                        for (int i = 0; i < chatrooms.Count; i++)
                        {
                            if (chatrooms.ElementAt(i).getRoomName().Equals(splitData[1]))
                            {
                                chatrooms.ElementAt(i).addUser(ID);
                                if (chatrooms.ElementAt(i).getUserList().Count == 2)
                                {
                                    chatrooms.ElementAt(i).createKalaha(chatrooms.ElementAt(i).getUserList().ElementAt(0), chatrooms.ElementAt(i).getUserList().ElementAt(1));
                                    Server.SendData(chatrooms.ElementAt(i).getUserList().ElementAt(0), ConvertStringToBytes("PLAYER_NAMES " + chatrooms.ElementAt(i).getUserList().ElementAt(0) + " " + chatrooms.ElementAt(i).getUserList().ElementAt(1)));
                                    Server.SendData(chatrooms.ElementAt(i).getUserList().ElementAt(1), ConvertStringToBytes("PLAYER_NAMES " + chatrooms.ElementAt(i).getUserList().ElementAt(0) + " " + chatrooms.ElementAt(i).getUserList().ElementAt(1)));
                                }
                                else if (chatrooms.ElementAt(i).getUserList().Count > 2)
                                {
                                    Server.SendData(ID, ConvertStringToBytes("PLAYER_NAMES " + chatrooms.ElementAt(i).getUserList().ElementAt(0) + " " + chatrooms.ElementAt(i).getUserList().ElementAt(1)));
                                }

                                Console.WriteLine(ID + " joined the chatroom " + chatrooms.ElementAt(i).getRoomName());
                                Server.SendData(ID, ConvertStringToBytes("JOINED_CHATROOM_CONFIRMED " + splitData[1]));
                                for (int j = 0; j < chatrooms.ElementAt(i).getUserList().Count; j++)
                                {
                                    Server.SendData(chatrooms.ElementAt(i).getUserList().ElementAt(j), ConvertStringToBytes("USER_JOINED " + ID));
                                }
                            }
                        }

                        break;
                    case "LEAVE_CHATROOM":          //A request to leave a chatroom should look like this: "LEAVE_CHATROOM *name*"

                        Boolean removeUserFound = false;

                        foreach (Chatroom chatroom in chatrooms)
                        {
                            foreach(string user in chatroom.getUserList())
                            {
                                if (user.Equals(ID))
                                {
                                    chatroom.removeUser(ID);
                                    Console.WriteLine(ID + " left the chatroom " + chatroom.getRoomName());
                                }
                                removeUserFound = true;
                                break;
                            }
                            if (removeUserFound)
                            {
                                break;
                            }
                        }
                        ClearEmptyRooms();
                        printChatrooms();
                        break;
                    case "SEND_MESSAGE":            //A request to send a message when in a chatroom should look like this: "SEND_MESSAGE *message*"

                        Boolean userFound = false;

                        for (int i = 0; i < chatrooms.Count; i++)
                        {
                            for (int j = 0; j < chatrooms.ElementAt(i).getUserList().Count; j++)
                            {
                                if (chatrooms.ElementAt(i).getUserList().ElementAt(j).Equals(ID))
                                {
                                    for (int h = 0; h < chatrooms.ElementAt(i).getUserList().Count; h++)
                                    {
                                        Server.SendData(chatrooms.ElementAt(i).getUserList().ElementAt(h), ConvertStringToBytes("SEND_MESSAGE " + ID + ": " + splitData[1]));
                                    }
                                    Console.WriteLine(ID + " send the message '" + splitData[1] + "' to the chatroom '" + chatrooms.ElementAt(i).getRoomName() + "'");
                                    userFound = true;
                                    break;
                                }
                            }
                            if (userFound)
                            {
                                break;
                            }
                        }

                        break;
                    case "REFRESH":
                        sendChatroomUpdate(ID);
                        break;
                    case "KALAHA":
                        Boolean playerFound = false;
                        for (int i = 0; i < chatrooms.Count; i++)
                        {
                            for (int j = 0; j < chatrooms.ElementAt(i).getUserList().Count; j++)
                            {
                                if (chatrooms.ElementAt(i).getUserList().ElementAt(j).Equals(ID))
                                {
                                    string temp = chatrooms.ElementAt(i).makeMove(Int32.Parse(splitData[1]));
                                    for (int h = 0; h < chatrooms.ElementAt(i).getUserList().Count; h++)
                                    {
                                        Server.SendData(chatrooms.ElementAt(i).getUserList().ElementAt(h), ConvertStringToBytes(temp));
                                    }
                                    playerFound = true;
                                    break;
                                }
                            }
                            if (playerFound)
                            {
                                break;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid messagetype in method 'Server_DataReceived'");
                        break;

                }
            }

            string ConvertBytesToString(byte[] bytes)
            {
                return ASCIIEncoding.ASCII.GetString(bytes);
            }

            byte[] ConvertStringToBytes(string str)
            {
                return ASCIIEncoding.ASCII.GetBytes(str);
            }
        }


    }
}
