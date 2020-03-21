using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Client
{
    class Program
    {
        private static TcpClient _tcpClient = new TcpClient();

        private static Guid _clientId = Guid.Empty;

        static void Main(string[] args)
        {
            Console.Title = "Client";
            ConnectLoop();
            WaitLoop();
            Send();
            GetResult();
            Console.ReadLine();
        }

        private static void GetResult()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int receivedData = _tcpClient.Client.Receive(buffer);
                if (receivedData != 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, receivedData);
                    Guid winnerId = Guid.Parse(result);
                    if (winnerId == _clientId)
                    {
                        Console.WriteLine("I am the winner!");
                    }
                    else
                    {
                        Console.WriteLine("I lost");
                    }
                    break;
                }
            }
        }

        private static void WaitLoop()
        {
            Console.Write("Waiting for game start");
            while (_clientId == Guid.Empty)
            {
                byte[] buffer = new byte[1024];
                int receivedData = _tcpClient.Client.Receive(buffer);
                if (receivedData != 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, receivedData);
                    Guid id = new Guid(result);
                    _clientId = id;

                }

            }
            Console.Write("Game started");
        }

        private static void Send()
        {
            int randomNumber = new Random().Next(0, 1000);
            Console.Write("Draw " + randomNumber + " as your random number");
            byte[] buffer = Encoding.ASCII.GetBytes(randomNumber.ToString());
            _tcpClient.Client.Send(buffer);
        }

        private static void ConnectLoop()
        {
            int attempts = 0;

            while (!_tcpClient.Connected)
            {

                try
                {
                    attempts++;
                    _tcpClient.Connect(IPAddress.Loopback, 5000);

                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts: " + attempts);
                }

            }
            Console.Clear();
            Console.WriteLine("Connected");
        }
    }
}
