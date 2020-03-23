using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static readonly TcpClient TcpClient = new TcpClient();

        private static  NetworkStream Stream => TcpClient.GetStream();

        private static Guid ClientId = Guid.Empty;

        static async Task Main(string[] args)
        {
            Console.Title = "Client";
            Console.WriteLine("Enter key to exit");
            Run();
            Console.ReadKey();
            Terminate("Closing client");
        }

        static async Task Run()
        {
            await ConnectLoop();
            await WaitLoop();
            await Send();
            await GetResult();
        }

        private static async Task GetResult()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int receivedData = await Stream.ReadAsync(buffer);
                if (receivedData != 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, receivedData);
                    Guid winnerId = Guid.Parse(result);
                    string message = winnerId == ClientId ? "I am the winner!" : "I lost ;(";
                    Terminate(message);
                }
            }
            catch (Exception exp)
            {
                Terminate(exp.Message);
            }
        }

        private static async Task WaitLoop()
        {
            Console.WriteLine("Waiting for game start");
            try 
            {
                byte[] buffer = new byte[1024];
                int received = await Stream.ReadAsync(buffer);
                if (received != 0)
                {
                    string result = Encoding.ASCII.GetString(buffer, 0, received);
                    Guid id = new Guid(result);
                    ClientId = id;
                    Console.WriteLine("Game started");
                }
            }
            catch (Exception exp)
            {
                Terminate(exp.Message);
            }
        }

        private static async Task Send()
        {
            int randomNumber = new Random().Next(0, 1000);
            Console.WriteLine("Draw " + randomNumber + " as your random number");
            byte[] buffer = Encoding.ASCII.GetBytes(randomNumber.ToString());
            await Stream.WriteAsync(buffer);
            await Stream.FlushAsync();
        }

        private static async Task ConnectLoop()
        {
            int attempts = 0;

            while (!TcpClient.Connected)
            {
                try
                {
                    attempts++;
                    await TcpClient.ConnectAsync(IPAddress.Loopback, 5000);
                }
                catch (Exception)
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
            if(TcpClient.Connected) Stream.Close();
            TcpClient.Close();
            if (TcpClient.Connected) Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
