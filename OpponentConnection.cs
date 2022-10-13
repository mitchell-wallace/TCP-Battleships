using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Battleships{

    internal class OpponentConnection{ // TODO: implement actual connection to opponent

        private static int counter = 0;

        public static bool FireAtOpponent(int column, int row) { // TODO: implement ship placement for dummy opponent
            if (column == 2) return true;
            return false;
        }

        public static async Task<bool> ListenAsHost() {

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
            }
            finally
            {
                listener.Stop();
            }

            await Task.Run( () => { // PLACEHOLDWER
                // PLACEHOLDER
            });


            return true;
        }

        public static async void InitiateAsClient() {
            var ipEndPoint = new IPEndPoint(Battleships.OpponentAddress, Battleships.AgreedTcpPort);

            using TcpClient client = new();
            await client.ConnectAsync(ipEndPoint);
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