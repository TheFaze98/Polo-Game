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

        private static readonly int _numberOfPlayers = 2;
        private static byte[] _buffer = new byte[1024];
        private static TcpListener _tcpListener = new TcpListener(IPAddress.Any, 5000);
        private static List<Player> _players = new List<Player>();

        static void Main(string[] args)
        {
            Console.Title = "Server";
            WaitForPlayers();
            SendIdForPlayers();
            WaitForNumbers();
            Player winner = GetWinner();
            SendResults(winner);
            Console.WriteLine("Enter to terminate");
            Console.ReadLine();
        }

        private static void SendResults(Player winner)
        {
            Console.WriteLine("Sending results");
            foreach (Player player in _players)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(winner.Id.ToString());
                player.Stream.Write(buffer);
                player.Stream.Flush();
            }
        }

        private static Player GetWinner()
        {
            Player winner = null;
            int maxNumber = 0;

            foreach (Player player in _players)
            {
                winner = player.RandomNumber > maxNumber ? player : winner;
            }
            return winner;
        }

        private static void WaitForNumbers()
        {
            Console.WriteLine("Collecting results from players");
            foreach (Player player in _players)
            {
                int received = player.Stream.Read(_buffer);
                if (received != 0)
                {
                    string result = Encoding.ASCII.GetString(_buffer, 0, received);
                    player.RandomNumber = Int32.Parse(result);
                    player.Stream.Flush();
                }
            }

        }

        private static void SendIdForPlayers()
        {
            Console.WriteLine("Sending ids for " + _numberOfPlayers + " players");

            foreach (Player player in _players)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(player.Id.ToString());
                player.Stream.Write(buffer);
                player.Stream.Flush();
            }
        }

        private static void WaitForPlayers()
        {
            Console.WriteLine("Wait for " + _numberOfPlayers + " players");
            _tcpListener.Start();
            WaitForPlayersAsync();
            Console.WriteLine("All players joined the server");
        }

        private static void WaitForPlayersAsync()
        {
            List<Player> players = new List<Player>();
            while (true)
            {
                Player player = new Player(_tcpListener.AcceptTcpClient());
                players.Add(player);
                Console.WriteLine("Player joined the server");
                if (players.Count >= _numberOfPlayers)
                {
                    _players = players;
                    break;
                }
            }
        }
    }
}
