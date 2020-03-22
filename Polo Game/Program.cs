using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Polo_Game
{
    class Program
    {
        private static readonly int _numberOfPlayers = 10;
        private static byte[] _buffer = new byte[1024];
        private static TcpListener _tcpListener = new TcpListener(IPAddress.Any, 5000);
        private static List<Player> _players = new List<Player>();

        static void Main(string[] args)
        {
            Console.Title = "Server";
            Run();
            Console.ReadLine();
        }

        private static async Task Run()
        {
            await WaitForPlayers();
            await SendIdForPlayersAsync();
            await WaitForNumbersAsync();
            Player winner = GetWinner();
            await SendResultsAsync(winner.Id);
            Console.WriteLine("Enter to terminate");
        }

        private static async Task SendResultsAsync(Guid winnerId)
        {
            Console.WriteLine("Sending results");
            foreach (Player player in _players)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(winnerId.ToString());
                await player.Stream.WriteAsync(buffer);
                await player.Stream.FlushAsync();
            }
        }

        private static Player GetWinner()
        {
            Player winner = _players[0];

            foreach (Player player in _players)
            {
                Console.WriteLine(_players.Count);
                winner = player.RandomNumber > winner.RandomNumber ? player : winner;
            }
            return winner;
        }

        private static async Task WaitForNumbersAsync()
        {
            Console.WriteLine("Collecting results from players");
            foreach (Player player in _players)
            {
                int received = await player.Stream.ReadAsync(_buffer);
                if (received != 0)
                {
                    string result = Encoding.ASCII.GetString(_buffer, 0, received);
                    player.RandomNumber = Int32.Parse(result);
                    await player.Stream.FlushAsync();
                }
            }

        }

        private static async Task SendIdForPlayersAsync()
        {
            Console.WriteLine("Sending ids for " + _numberOfPlayers + " players");

            foreach (Player player in _players)
            {
                if (player.TcpClient.Connected)
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(player.Id.ToString());
                    await player.Stream.WriteAsync(buffer);
                    await player.Stream.FlushAsync();
                }
                else
                {
                    Console.WriteLine("Player disconnected");
                }
            }
        }

        private static async Task WaitForPlayers()
        {
            _tcpListener.Start();
            Console.WriteLine("Wait for " + _numberOfPlayers + " players");
            while (true)
            {
                Player player = new Player(await _tcpListener.AcceptTcpClientAsync());
                _players.Add(player);
                Console.WriteLine("Player joined the server");
                if (_players.Count == _numberOfPlayers)
                {
                    Console.WriteLine("All players joined the server");
                    break;
                }
            }
        }

        private static void Terminate(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static bool IsConnected(TcpClient client)
        {
            try
            {
                if (client != null && client.Client != null && client.Client.Connected)
                {
                    if (client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            
        }
    }
}
