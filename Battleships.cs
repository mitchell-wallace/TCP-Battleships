using System.Net;

namespace Battleships
{
    public class Battleships
    {
        public static IPAddress BcAddress = IPAddress.Parse("127.0.0.1"); // UDP Broadcast Address; overwritten by arguments
        public static IPAddress OpponentAddress = IPAddress.Parse("127.0.0.1"); // TCP opponent address
        public static int BcPort = 9000; // UDP Broadcast Port
        public static int RandomTcpPort = 5001; // Randomised TCP port decided by this instance
        public static int AgreedTcpPort = 5001; // Agreed-upon TCP port by both instances
        public static int PlayerNo = 0; // 0 = unassigned; 1 = host; 2 = client
        public static string PlayerName = "Battleships Player"; // initialise to a placeholder because it feels safer this way
        public static string OpponentName = "Battleships Player"; // initialise to a placeholder because it feels safer this way
        public static bool GameOver = false; // when set to true, the game will close down
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n> > > >   B a t t l e s h i p s ! !   < < < <\n\n");
            Console.ResetColor();

            // Set random TCP port
            RandomTcpPort = new Random().Next(5000, 6000); 
                // retain this even after setting agreed port,
                // for filtering UDP messages we sent. probably redundant.
            AgreedTcpPort = RandomTcpPort; // assume we are host; we will overwrite otherwise

            // Broadcast settings
            if (args.Length > 1)
            {
                BcAddress = IPAddress.Parse(args[0]);
                OpponentAddress = BcAddress;
                BcPort = int.Parse(args[1]);
                if (args.Length > 2)
                {
                    if (args[2].Length > 0)
                    PlayerName = args[2];
                    if (PlayerName.Length > 64) PlayerName = PlayerName.Replace(" ", "")[0..64]; // trim playernames
                    Console.WriteLine($"Welcome, {PlayerName}!");
                }
                else
                {
                    Console.WriteLine("Welcome! Did you know you can set a custom " +
                        "player name using the third argument on the command line?");
                    RandomisePlayerName();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No command line arguments received; please specify broadcast address and port." +
                "\nAttempting to play using default settings; press CTRL+C to cancel.");
                Console.ResetColor();
                RandomisePlayerName();
            }
            Console.WriteLine($"Broadcast address: {BcAddress} | Broadcast port: {BcPort} | " +
                $"Randomised TCP port: {RandomTcpPort}\nPlayer name: {PlayerName}\n");

            // Game search
            do
            {
                Broadcast.Connect();
            } while (PlayerNo == 0); // looping here is really just a backup that should never have to happen

            // at this point, we cannot yet guarantee that the TCP connection has fully initialised!
            while (!OpponentConnection.TcpEstablished) Thread.Sleep(1000); // wait until handshake is fully completed
            Console.WriteLine($"\nNow playing as player #{PlayerNo}");
            Console.WriteLine($"Your opponent is: {OpponentName}");

            // main game method
            UserInterface.Play(PlayerNo == 1);

            // shutdown after game finished
            Shutdown();
        }

        private static void RandomisePlayerName()
        {
            PlayerName = "Battleships_Player" + new Random().Next(100, 999);
        }

        public static void Shutdown() 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nShutting down game...");
            Console.ResetColor();
            OpponentConnection.Close();
            Environment.Exit(0);
        }
    }
}