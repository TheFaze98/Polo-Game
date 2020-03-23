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
        private static readonly int NumberOfPlayers = 2;

        private static TCPServer Server;
        private static List<Player> Players => Server.Players;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 5000);
            Server = new TCPServer(tcpListener, NumberOfPlayers);

            Console.WriteLine("Server started");
            Run();

            Console.WriteLine("Enter key to end the game and close the server");
            Console.ReadKey();
            Server.Terminate("Server closed");
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
            foreach (Player player in Players)
            {
               await Server.SendAsync(player.Stream, winnerId.ToString());
            }
            Server.Terminate("Game ended");
        }

        private static Player GetWinner()
        {
            Player winner = Server.Players[0];

            foreach (Player player in Players)
            {
                winner = player.RandomNumber > winner.RandomNumber ? player : winner;
            }
            return winner;
        }

        private static async Task WaitForNumbersAsync()
        {
            Console.WriteLine("Collecting results from players");
            foreach (Player player in Players)
            {
                await Server.GetRandomNumberAsync(player);
            }

        }

        private static async Task SendIdForPlayersAsync()
        {
            Console.WriteLine("Sending ids for " + NumberOfPlayers + " players");
            foreach (Player player in Players)
            {
                if (player.TcpClient.Connected)
                {
                    await Server.SendAsync(player.Stream, player.Id.ToString());
                }
                else
                {
                    Console.WriteLine("Player disconnected");
                }
            }

        }

        private static async Task WaitForPlayers()
        {
            Console.WriteLine("Wait for " + NumberOfPlayers + " players");
            await Server.WaitForPlayers().ContinueWith(x => Console.WriteLine("All players joined the server"));
        }

    }
}
