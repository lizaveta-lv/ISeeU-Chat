﻿using ChatServer.Net.IO;
using System;
using System.Net;
using System.Net.Sockets;


namespace ChatServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true) 
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);
                /*Broadcast connect to the server*/
                BroadcastConnection();
                

            }
            
            
        }

        //Remake to be more efficient
        static void BroadcastConnection() 
        {
            foreach (var user in _users) 
            { 
                foreach (var u in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(u.Username);
                    broadcastPacket.WriteMessage(u.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }

        public static void BroadcastMessage(string message)
        {
            foreach (var user in _users) 
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }

        }
        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
               
            }

            BroadcastMessage($"[{disconnectedUser.Username}] disconnected.");

        }
    }
}
