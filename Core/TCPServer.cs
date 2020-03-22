using System;
using System.Net.Sockets;

namespace Core
{
    public class TCPServer
    {
        private TcpListener _tcpListener;

        public TCPServer(TcpListener tcpLuListener)
        {
            this._tcpListener = tcpLuListener;

        }
    }
}
