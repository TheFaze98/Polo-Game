using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Polo_Game
{
    public class Player
    {
        public TcpClient TcpClient { get; }

        public NetworkStream Stream => this.TcpClient.GetStream();

        public Guid Id { get; }

        public int RandomNumber { get; set; }

        public Player(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
            this.Id = Guid.NewGuid();
        }

        public Player(TcpClient tcpClient, Guid guid)
        {
            this.TcpClient = tcpClient;
            this.Id = guid;
        }
    }
}