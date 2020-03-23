using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polo_Game;

namespace Core
{
    public class TCPServer
    {
        private readonly TcpListener TcpListener;
        private readonly int NumberOfPlayers;
        private byte[] Buffer = new byte[1024];
        public List<Player> Players = new List<Player>();

        public TCPServer(TcpListener tcpLuListener, int numberOfPlayers)
        {
            this.TcpListener = tcpLuListener;
            this.NumberOfPlayers = numberOfPlayers;
            TcpListener.Start();
        }

        public async Task SendAsync(NetworkStream stream, string message)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(buffer);
                await stream.FlushAsync();
            }
            catch (Exception exp)
            {
                Terminate(exp.Message);
            }
        }

        public async Task GetRandomNumberAsync(Player player)
        {
            try
            {
                int received = await player.Stream.ReadAsync(Buffer);
                if(received != 0)
                {
                    string result = Encoding.ASCII.GetString(Buffer, 0, received);
                    player.RandomNumber = Int32.Parse(result);
                    await player.Stream.FlushAsync();
                }
            }
            catch(Exception exp)
            {
                Terminate(exp.Message);
            }
        }

        public async Task WaitForPlayers()
        {
            var cancellation = new CancellationTokenSource();
            cancellation.Token.Register(() => TcpListener.Stop());
            try
            {
                while (true)
                {
                    Player player = new Player(await Task.Run(() => TcpListener.AcceptTcpClientAsync(), cancellation.Token));
                    Players.Add(player);
                    Console.WriteLine("Player joined the server");
                    if (Players.Count == NumberOfPlayers)
                    {
                        cancellation.Cancel();
                        break;
                    }
                }
            }
            finally
            {
                TcpListener.Stop();
            }
        }

        public void Terminate(string message)
        {
            Console.WriteLine(message);
            foreach (Player player in Players)
            {
                if(player.TcpClient.Connected) player.Stream.Close();
                player.TcpClient.Close();
            }
            Console.WriteLine("Enter to exit");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
