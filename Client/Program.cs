using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static TcpClient _tcpClient = new TcpClient();

        private static Guid _clientId = Guid.Empty;

        static async Task Main(string[] args)
        {
            Console.Title = "Client";
            Run();
            Console.ReadKey();
        }

        static async Task Run()
        {
            Console.Title = "Client";
            ConnectLoop();
            await WaitLoop();
            await Send();
            await GetResult();
        }

        private static async Task GetResult()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                NetworkStream stream = _tcpClient.GetStream();
                int receivedData = await stream.ReadAsync(buffer);
                if (receivedData != 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, receivedData);
                    Guid winnerId = Guid.Parse(result);
                    string message = winnerId == _clientId ? "I am the winner!" : "I lost";
                    Terminate(message);
                }
            }
        }

        private static async Task WaitLoop()
        {
            Console.WriteLine("Waiting for game start");
            while (_clientId == Guid.Empty)
            {
                byte[] buffer = new byte[1024];
                NetworkStream stream = _tcpClient.GetStream();
                int received = await stream.ReadAsync(buffer);
                if (received != 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, received);
                    Guid id = new Guid(result);
                    _clientId = id;
                    Console.WriteLine("Game started");
                }

            }
        }

        private static async Task Send()
        {
            int randomNumber = new Random().Next(0, 1000);
            Console.WriteLine("Draw " + randomNumber + " as your random number");
            byte[] buffer = Encoding.ASCII.GetBytes(randomNumber.ToString());
            NetworkStream stream = _tcpClient.GetStream();
            await stream.WriteAsync(buffer);
            await stream.FlushAsync();
        }

        private static void ConnectLoop()
        {
            int attempts = 0;

            while (!_tcpClient.Connected)
            {

                try
                {
                    attempts++;
                    _tcpClient.ConnectAsync(IPAddress.Loopback, 5000);

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

        private static void Terminate(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
