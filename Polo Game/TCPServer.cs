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
        private readonly TcpListener _tcpListener;
        private readonly int _numberOfPlayers;
        private byte[] _buffer = new byte[1024];
        public List<Player> _players = new List<Player>();

        public TCPServer(TcpListener tcpLuListener, int numberOfPlayers)
        {
            this._tcpListener = tcpLuListener;
            this._numberOfPlayers = numberOfPlayers;
            _tcpListener.Start();
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
                int received = await player.Stream.ReadAsync(_buffer);
                if(received != 0)
                {
                    string result = Encoding.ASCII.GetString(_buffer, 0, received);
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
            cancellation.Token.Register(() => _tcpListener.Stop());
            try
            {
                while (true)
                {
                    Player player = new Player(await Task.Run(() => _tcpListener.AcceptTcpClientAsync(), cancellation.Token));
                    _players.Add(player);
                    Console.WriteLine("Player joined the server");
                    if (_players.Count == _numberOfPlayers)
                    {
                        cancellation.Cancel();
                        break;
                    }
                }
            }
            finally
            {
                _tcpListener.Stop();
            }
        }

        public void Terminate(string message)
        {
            Console.WriteLine(message);
            foreach (Player player in _players)
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
