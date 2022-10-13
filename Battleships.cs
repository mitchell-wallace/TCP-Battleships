

using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Battleships {

    public class Battleships {

        public static IPAddress BcAddress = IPAddress.Parse("127.0.0.1"); // UDP Broadcast Address; overwritten by arguments
        public static IPAddress OpponentAddress = IPAddress.Parse("127.0.0.1"); // TCP opponent address; set to placeholder
        public static int BcPort = 9000; // UDP Broadcast Port
        public static int RandomTcpPort = 9001; // Randomised TCP port decided by this instance
        public static int AgreedTcpPort = 9001; // Agreed-upon TCP port by both instances
        public static int PlayerNo = 0; // 0 = unassigned; 1 = host; 2 = client
        public static string PlayerName = "Battleships Player";

        public static void Main(string[] args) {

            Console.WriteLine("> > > >   B a t t l e s h i p s ! !   < < < <\n\n");

            // Set random TCP port
            RandomTcpPort = new Random().Next(1024, 65000); // retain this for filtering UDP messages we sent. probably redundant.
            AgreedTcpPort = RandomTcpPort; // assume we are host; we will overwrite otherwise

            // Broadcast settings
            if (args.Length > 1)
            {
                BcAddress = IPAddress.Parse(args[0]);
                BcPort = int.Parse(args[1]);
                if (args.Length > 2)
                {
                    PlayerName = args[2];
                    Console.WriteLine($"Welcome, {PlayerName}!");
                }
                else
                {
                    Console.WriteLine("Welcome! Did you know you can set a custom " +
                        "player name using the third argument on the command line?");
                }
                    
            }    
            else
            {
                Console.WriteLine("No command line arguments received; please specify broadcast address and port." +
                "\nAttempting to play using default settings; press CTRL+C to cancel.");
            }
            Console.WriteLine($"Broadcast address: {BcAddress} | Broadcast port: {BcPort} | Randomised TCP port: {RandomTcpPort}\n");

            // Game search
            
            Broadcast.Connect();
            Console.WriteLine("No existing game found; hosting new game");
            Console.WriteLine("Waiting for opponent to join... <NOT IMPLEMENTED>");
            Console.WriteLine("Now playing against: <DUMBEST POSSIBLE OPPONENT>");

            UserInterface.Play(true); // FIXME for testing, we are player1; this won't work with two real players!

        }

        public static void Connect()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, BcPort));

            var from = new IPEndPoint(0, 0);
            var task = Task.Run(() =>
            {
                while (true)
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    Console.WriteLine(Encoding.UTF8.GetString(recvBuffer));
                }
            });

            var data = Encoding.UTF8.GetBytes("ABCD");
            udpClient.Send(data, data.Length, "255.255.255.255", BcPort);

            task.Wait();
        }




    }



}