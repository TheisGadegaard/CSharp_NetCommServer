using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCommServer
{
    class Chatroom
    {
        private string roomName;
        private Kalaha kalaha;
        private List<string> userList = new List<string>();
        private List<string> playerList = new List<string>();

        public Chatroom(string roomName)
        {
            this.roomName = roomName;
        }

        public void createKalaha(string player1, string player2)
        {
            kalaha = new Kalaha(player1, player2);
        }

        public void addUser(string ID)
        {
            userList.Add(ID);
        }

        public string makeMove(int move)
        {
            return kalaha.makeMove(move);
        }

        public string getRoomName()
        {
            return roomName;
        }
        public List<string> getUserList()
        {
            return userList;
        }

        public void removeUser(string ID)
        {
            userList.Remove(ID);
            playerList.Remove(ID);
        } 
    }
}
