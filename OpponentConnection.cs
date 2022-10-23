using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace Battleships
{
    internal class OpponentConnection
    {
        private static TcpClient? tcpClient = null;
        private static IPEndPoint? opponentEndpoint = null;
        private static NetworkStream? tcpStream = null;
        private static bool sendReceiveReady = false; // true when we are ready to use Send() and Receive()
        public static bool TcpEstablished = false; // true when all components of handshake completed
        private static int bufferSize = 256; // it's small but it's enough
        public static string FireAtOpponent(int column, int row)
            // send FIRE message to opponent and listen for result
        {
            string result = "";
            try 
            {
                Send($"FIRE:{CharTransform.ColumnChar(column)}{row + 1}");
                result = Receive();

                if (result.Length > 9 && result[0..9] == "GAME OVER") 
                {
                    Battleships.GameOver = true;
                    UserInterface.gg.FireShot(column, row, true);
                }
                else if (result[0..4] == "MISS") UserInterface.gg.FireShot(column, row, false);
                else UserInterface.gg.FireShot(column, row, true);
            }
            catch (Exception e) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn exception occurred while firing at opponent:\n{e}");
                Console.ResetColor();
                Battleships.Shutdown(); // if exceptions come back to here, the connection is proper dead
            }
            return result;
        }

        public static void Close()
            // close connection in a clean shutdown
        {
            if (tcpClient is not null) tcpClient.Close();
        }

        public static string OpponentFiresAtUs()
            // listen to FIRE message from opponent and respond with result
        {
            string result = "";

            try 
            {
                string receiveString = Receive();
                if (receiveString == "END") 
                {
                    Console.WriteLine("Opponent has left the game. Thanks for playing!");
                    Battleships.Shutdown();
                }

                int column = CharTransform.ColumnNo(receiveString[5]);
                int row = int.Parse(receiveString[6..]) - 1;
                char resultChar = UserInterface.gg.ReceiveShot(column, row);

                if (resultChar == 'X')
                {
                    result = $"HIT:{receiveString[5..]}";
                }
                else if (resultChar == '~')
                {
                    result = $"MISS:{receiveString[5..]}";
                }
                else 
                {
                    if (resultChar == resultChar.ToString().ToLower()[0]) // lowercase indicates game over
                    {
                        result = $"GAME OVER:{receiveString[5..]}:" + 
                            $"{CharTransform.ShipType(resultChar.ToString().ToUpper()[0])}";
                        Battleships.GameOver = true;
                    }
                    else
                        result = $"SUNK:{receiveString[5..]}:{CharTransform.ShipType(resultChar)}";
                }

                Send(result);
            }
            catch (Exception e) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn exception occurred while being fired at by opponent:\n{e}");
                Console.ResetColor();
                Battleships.Shutdown(); // if exceptions come back to here, the connection is proper dead
            }

            return result;
        }

        public static async void InitialListen() 
            // run as thread; listen for another instance connecting to your broadcasted TCP port
        {
            string message = "";

            await Task.Run(async () => // probably redundant when we run this as a thread already
            {
                opponentEndpoint = new IPEndPoint(IPAddress.Any, Battleships.AgreedTcpPort);
                TcpListener listener = new(opponentEndpoint);

                try
                {
                    listener.Start();

                    TcpClient handler = await listener.AcceptTcpClientAsync();
                    tcpStream = handler.GetStream();

                    // *~*~* RECEIVING MESSAGE *~*~*
                    var buffer = new byte[bufferSize];
                    int received = await tcpStream.ReadAsync(buffer);
                    message = Encoding.UTF8.GetString(buffer, 0, received);

                    tcpClient = handler;
                    sendReceiveReady = true;
                }
                catch (Exception e) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nAn exception occurred while receiving a TCP message from opponent:\n{e}");
                    Console.ResetColor();
                }
                finally
                {
                    listener.Stop();
                }
            });

            if (message.Length > 11 && message[0..11] == "GAME START:")
            {
                Battleships.OpponentName = message[11..];
                Battleships.PlayerNo = 1;
                Broadcast.contacted = true; // this will break the UDP listen-send loop
                Send($"GAME START:{Battleships.PlayerName}");
                TcpEstablished = true;
            }
        }
 
        public static string Receive()
            // wait to receive a TCP message from your opponent; this is intentionally synchronous
        {
            if (!sendReceiveReady) { // this will not be called until after TCP has begun to be established; wait should be short!
                for (int i = 0; i <= 240; i++) {
                    if (sendReceiveReady) break;
                    if (i != 0 && i%60 == 0) 
                        Console.WriteLine("INFO: TCP message send delayed; awaiting ready flag");
                    if (i == 240) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nMessage timeout! Critical connection error.");
                        Console.ResetColor();
                        Battleships.Shutdown();
                    }
                    Thread.Sleep(1000);
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.Assert(tcpClient is not null, "tcpClient has not initialised!");
            Debug.Assert(opponentEndpoint is not null, "opponentEndpoint has not initialised!");
            Debug.Assert(tcpStream is not null, "tcpStream has not initialised!");
            Console.ResetColor();

            string message = "";
            TcpListener listener = new(opponentEndpoint);

            try
            {
                // *~*~* RECEIVING MESSAGE *~*~*
                var buffer = new byte[bufferSize];
                int received = tcpStream.Read(buffer);

                message = Encoding.UTF8.GetString(buffer, 0, received);
            }
            catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn exception occurred while receiving a TCP message from opponent:\n{e}");
                Console.ResetColor();
            }
            finally
            {
                listener.Stop();
            }

            return message;
        }

        public static async void Send(string msg) 
            // send a message to your opponent
        {
            if (!sendReceiveReady) { // this will not be called until after TCP has begun to be established; wait should be short!
                for (int i = 0; i <= 240; i++) {
                    if (sendReceiveReady) break;
                    if (i != 0 && i%60 == 0) 
                        Console.WriteLine("INFO: TCP message send delayed; awaiting ready flag");
                    if (i == 240) Console.WriteLine("Message timeout! Critical connection error.");
                    Thread.Sleep(1000);
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.Assert(tcpClient is not null, "tcpClient has not initialised!");
            Debug.Assert(opponentEndpoint is not null, "opponentEndpoint has not initialised!");
            Debug.Assert(tcpStream is not null, "tcpStream has not initialised!");
            Console.ResetColor();

            try
            {
                // *~*~* SENDING MESSAGE *~*~*
                NetworkStream stream = new NetworkStream(tcpClient.Client);
                var messageBytes = Encoding.UTF8.GetBytes(msg);
                await stream.WriteAsync(messageBytes);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn error occurred while sending a message to Player 1: \n" + 
                    $"Message text: {msg}\n{e}");
                Console.ResetColor();
                Battleships.Shutdown();
            }
        }

        public static async void InitiateAsClient()
            // after receiving the broadcast from another instance, broadcast a message on TCP
        {
            opponentEndpoint = new IPEndPoint(Battleships.OpponentAddress, Battleships.AgreedTcpPort);

            TcpClient client = new();
            try
            {
                await client.ConnectAsync(opponentEndpoint);
                tcpStream = client.GetStream();

                // *~*~* SENDING MESSAGE *~*~*
                var message = $"GAME START:{Battleships.PlayerName}";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await tcpStream.WriteAsync(messageBytes);

                tcpClient = client;
                sendReceiveReady = true;

                // wait for response with playername
                Battleships.OpponentName = Receive()[11..];
                TcpEstablished = true;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn error occurred while initiating connection to Player 1: {e}");
                Console.ResetColor();
                Battleships.Shutdown();
            }
        }
    }
}