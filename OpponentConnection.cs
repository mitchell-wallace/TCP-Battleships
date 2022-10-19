using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Battleships
{

    internal class OpponentConnection
    { // TODO: implement actual connection to opponent

        private static int counter = 0;

        public static bool FireAtOpponent(int column, int row)
        {
            if (column == 2) return true;
            return false;
        }

        public static async void HostListen()
        {
            var receiveString = await TcpListenThread();
            if (receiveString == "GAME START")
            {
                Console.WriteLine("====> HostListen complete <====");
                Battleships.PlayerNo = 1;
                Broadcast.contacted = true; // this will break the UDP listen-send loop
            }
        }

        public static async Task<string> TcpListenThread() // do we constantly listen, or only when we expect a message???
                                                           // I think only when we expect a message, and then we get TcpListen to return the message
        {
            string message = "";
            await Task.Run(async () =>
            { // PLACEHOLDWER
                // PLACEHOLDER
                // FIXME we're running this as a thread instead so this whole method can probs
                // be written as synchronous

                Console.WriteLine("====> Executing OpponentConnection.ListenAsHost() <====");
                var ipEndPoint = new IPEndPoint(IPAddress.Any, Battleships.AgreedTcpPort);
                TcpListener listener = new(ipEndPoint);

                try
                {
                    listener.Start();

                    using TcpClient handler = await listener.AcceptTcpClientAsync();
                    await using NetworkStream stream = handler.GetStream();

                    // *~*~* RECEIVING MESSAGE *~*~*
                    var buffer = new byte[1_024];
                    int received = await stream.ReadAsync(buffer);

                    message = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Message received: \"{message}\"");

                    // TODO: actually check the logic that the game starts after this point?!?!
                }
                finally
                {
                    listener.Stop();
                }


            });
            return message;
        }

        public static async void InitiateAsClient()
        {
            var opponentEndPoint = new IPEndPoint(Battleships.OpponentAddress, Battleships.AgreedTcpPort);

            using TcpClient client = new();
            try
            {
                await client.ConnectAsync(opponentEndPoint);
                await using NetworkStream stream = client.GetStream();

                // *~*~* SENDING MESSAGE *~*~*
                var message = "GAME START";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBytes);

                Console.WriteLine($"Sent message: \"{message}\"");

                // *~*~* SENDING SECOND MESSAGE *~*~*
                Thread.Sleep(1500);
                message = "SECOND MESSAGE";
                messageBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBytes);

                Console.WriteLine($"Sent message: \"{message}\"");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while initiating connection to Player 1: {e}");
                Console.WriteLine("\n!IMPORTANT! Restart this client to reattempt connection.");
            }
        }

        public static int[] OpponentFiresAtUs()
        {
            int[] result = { counter % 10, counter / 10 };
            counter++;
            return result;
        }

        public static void ResponseToOpponentShot(string response)
        {
            // TODO: respond to opponent with MISS, HIT, SUNK messages
        }

    }
}