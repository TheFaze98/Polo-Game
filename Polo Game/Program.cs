using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core;

namespace Polo_Game
{

    class Program
    {
        private static readonly int _numberOfPlayers = 4;

        private static TCPServer _server;
        private static List<Player> _players => _server._players;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 5000);
            _server = new TCPServer(tcpListener, _numberOfPlayers);

            Console.WriteLine("Server started");
            Run();

            Console.WriteLine("Enter key to end the game and close the server");
            Console.ReadKey();
            _server.Terminate("Server closed");
        }

        private static async Task Run()
        {
            await WaitForPlayers();
            await SendIdForPlayersAsync();
            await WaitForNumbersAsync();
            Player winner = GetWinner();
            await SendWinnderIdToPlayers(winner.Id);
        }

        private static async Task SendWinnderIdToPlayers(Guid winnerId)
        {
            Console.WriteLine("Sending results");
            foreach (Player player in _players)
            {
               await _server.SendAsync(player.Stream, winnerId.ToString());
            }
            _server.Terminate("Game ended");
        }

        private static Player GetWinner()
        {
            Player winner = _server._players[0];

            foreach (Player player in _players)
            {
                winner = player.RandomNumber > winner.RandomNumber ? player : winner;
            }
            return winner;
        }

        private static async Task WaitForNumbersAsync()
        {
            Console.WriteLine("Collecting results from players");
            foreach (Player player in _players)
            {
                await _server.GetRandomNumberAsync(player);
            }

        }

        private static async Task SendIdForPlayersAsync()
        {
            Console.WriteLine("Sending ids for " + _numberOfPlayers + " players");
            foreach (Player player in _players)
            {
                if (player.TcpClient.Connected)
                {
                    await _server.SendAsync(player.Stream, player.Id.ToString());
                }
                else
                {
                    Console.WriteLine("Player disconnected");
                }
            }

        }

        private static async Task WaitForPlayers()
        {
            Console.WriteLine("Wait for " + _numberOfPlayers + " players");
            await _server.WaitForPlayers().ContinueWith(x => Console.WriteLine("All players joined the server"));
        }

    }
}
