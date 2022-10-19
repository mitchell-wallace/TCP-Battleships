using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace Battleships
{

    internal class OpponentConnection
    { // TODO: implement actual connection to opponent

        private static int counter = 0;
        private static TcpClient? tcpClient = null;
        private static IPEndPoint? opponentEndpoint = null;
        private static NetworkStream? tcpStream = null;
        private static bool ready = false;

        public static bool FireAtOpponent(int column, int row)
        {
            if (column == 2) return true;
            return false;
        }

        public static void Close()
        {
            if (tcpClient is not null) tcpClient.Close();
        }

        public static async void HostListen()
        {
            var receiveString = await TcpInitialListenThread();
            if (receiveString == "GAME START")
            {
                Console.WriteLine("====> HostListen complete <====");
                Battleships.PlayerNo = 1;
                Broadcast.contacted = true; // this will break the UDP listen-send loop
            }
        }

        public static async Task<string> TcpInitialListenThread()
        {
            string message = "";
            await Task.Run(async () =>
            {
                Console.WriteLine("====> Executing OpponentConnection.ListenAsHost() <====");
                opponentEndpoint = new IPEndPoint(IPAddress.Any, Battleships.AgreedTcpPort);
                TcpListener listener = new(opponentEndpoint);

                try
                {
                    listener.Start();

                    TcpClient handler = await listener.AcceptTcpClientAsync();
                    tcpStream = handler.GetStream();

                    // *~*~* RECEIVING MESSAGE *~*~*
                    var buffer = new byte[1_024];
                    int received = await tcpStream.ReadAsync(buffer);

                    message = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Message received: \"{message}\"");

                    tcpClient = handler;
                    ready = true;

                    // TODO: actually check the logic that the game starts after this point?!?!
                }
                finally
                {
                    listener.Stop();
                }
            });
            return message;
        }

        public static async void SendTcpMsg(string msg) {
            if (!ready) { // this will not be called until after TCP has begun to be established; wait should be short!
                for (int i = 0; i <= 240; i++) {
                    if (ready) break;
                    if (i%30 == 0) Console.WriteLine("INFO: TCP message send delayed; awaiting ready flag");
                    if (i == 240) Console.WriteLine("Message timeout! Critical connection error.");
                    Thread.Sleep(1000);
                }
            }
            Debug.Assert(tcpClient is not null, "tcpClient has not initialised!");
            Debug.Assert(opponentEndpoint is not null, "opponentEndpoint has not initialised!");
            Debug.Assert(tcpStream is not null, "tcpStream has not initialised!");

            try
            {
                // *~*~* SENDING MESSAGE *~*~*
                NetworkStream stream = new NetworkStream(tcpClient.Client);

                var messageBytes = Encoding.UTF8.GetBytes(msg);
                await stream.WriteAsync(messageBytes);

                Console.WriteLine($"Sent message: \"{msg}\"");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while sending a message to Player 1: \n" + 
                    $"Message text: {msg}\n{e}");
                Console.WriteLine("\n!IMPORTANT! Restart this client to reattempt connection.");
                Battleships.Shutdown();
            }

        }

        public static async void InitiateAsClient()
        {
            opponentEndpoint = new IPEndPoint(Battleships.OpponentAddress, Battleships.AgreedTcpPort);

            TcpClient client = new();
            try
            {
                await client.ConnectAsync(opponentEndpoint);
                tcpStream = client.GetStream();

                // *~*~* SENDING MESSAGE *~*~*
                var message = "GAME START";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await tcpStream.WriteAsync(messageBytes);

                Console.WriteLine($"Sent message: \"{message}\"");

                tcpClient = client;
                ready = true;
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