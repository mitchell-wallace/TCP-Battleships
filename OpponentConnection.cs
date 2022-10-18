using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Battleships{

    internal class OpponentConnection{ // TODO: implement actual connection to opponent

        private static int counter = 0;

        public static bool FireAtOpponent(int column, int row) {
            if (column == 2) return true;
            return false;
        }

        public static async void ListenAsHost() 
        { // FIXME listen as a loop!!! ---- actually I think TcpListener.Start() doesn't need to be looped

            Console.WriteLine("====> Executing OpponentConnection.ListenAsHost() <====");
            var ipEndPoint = new IPEndPoint(IPAddress.Any, Battleships.AgreedTcpPort);
            TcpListener listener = new(ipEndPoint);

            try
            {    
                listener.Start();

                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await using NetworkStream stream = handler.GetStream();

                // *~*~* SENDING MESSAGE *~*~*
                // var message = $"📅 {DateTime.Now} 🕛";
                // var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                // await stream.WriteAsync(dateTimeBytes);

                // Console.WriteLine($"Sent message: \"{message}\"");

                // *~*~* RECEIVING MESSAGE *~*~*
                var buffer = new byte[1_024];
                int received = await stream.ReadAsync(buffer);

                var message = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"Message received: \"{message}\"");
                Battleships.PlayerNo = 1;
                // TODO: actually check the logic that the game starts after this point?!?!
            }
            finally
            {
                listener.Stop();
            }

            await Task.Run( () => { // PLACEHOLDWER
                // PLACEHOLDER
                // FIXME we're running this as a thread instead so this whole method can probs
                // be written as synchronous
            });

        }

        public static async void InitiateAsClient() { // DEBUG this connection is being actively refused...
            var opponentEndPoint = new IPEndPoint(Battleships.OpponentAddress, Battleships.AgreedTcpPort);

            using TcpClient client = new();
            try {
                await client.ConnectAsync(opponentEndPoint); // this is the line that is breaking currently
                await using NetworkStream stream = client.GetStream();

                // *~*~* SENDING MESSAGE *~*~*
                var message = $"📅 {DateTime.Now} 🕛";
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes);

                Console.WriteLine($"Sent message: \"{message}\"");

                // *~*~* RECEIVING MESSAGE *~*~*
                // var buffer = new byte[1_024];
                // int received = await stream.ReadAsync(buffer);

                // var message = Encoding.UTF8.GetString(buffer, 0, received);
                // Console.WriteLine($"Message received: \"{message}\"");
            }
            catch (Exception e) {
                Console.WriteLine($"An error occurred while initiating connection to Player 1: {e}");
                Console.WriteLine("\n!IMPORTANT! Restart this client to reattempt connection.");
            }
        }

        public static int[] OpponentFiresAtUs()
        {
            int[] result = { counter % 10, counter / 10};
            counter++;
            return result;
        }

        public static void ResponseToOpponentShot(string response)
        {
            // TODO: respond to opponent with MISS, HIT, SUNK messages
        }

    }
}