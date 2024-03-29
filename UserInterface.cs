using System.Text.RegularExpressions;

namespace Battleships
{
    internal class UserInterface
    {
        static bool StillPlaying = true; // loop break variable
        static string whoseTurn = ""; // solely for display
        static int turnNo = 1; // primarily for display; simpler to start from 1
        public static GameGrids gg = new GameGrids(); // data structure for game data
        static Regex ShootingRegex = new Regex(@"^[A-J](10|[1-9])$"); // regex pattern for validating shooting input
        static Regex PlacementRegex = new Regex(@"^[A-J](10|[1-9])(VH)$"); // regex pattern for validating ship placement input

        public static void Play(bool isPlayer1)
            // isPlayer1 determines whether odd-numbered turns or even-numbered turns are our turn to shoot
        { 
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("You may send the message 'END' at any time to end the game.\n");
            Console.ResetColor();

            // place ships
            PlaceShipsRandomly();

            while (StillPlaying)
            {
                // logic for alternating turns
                bool yourTurn = (isPlayer1 && turnNo % 2 == 1 || !isPlayer1 && turnNo % 2 == 0);
                if (yourTurn) whoseTurn = "YOUR turn";
                else whoseTurn = "OPPONENT'S turn";

                // info display
                string info1 = $"======= Turn number {turnNo} : {whoseTurn} =======";
                string info2 = new string('=', info1.Length);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n\n{info2}\n{info1}\n{info2}\n");
                Console.ResetColor();

                // call code for the turn
                if (yourTurn) YourTurn();
                else OpponentsTurn();

                // end of turn process
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n" + gg.ToString());
                Console.ResetColor();
                if (Battleships.GameOver) GameOver(yourTurn); // check for game over
                turnNo++;
            }
        }

        public static void GameOver(bool yourTurn)
            // for game over, display farewell message and shut down gracefully
        {
            if (yourTurn) 
            {   
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Game over, you win against {Battleships.OpponentName}!" + 
                    $"\nThanks for playing, {Battleships.PlayerName}.");
                Console.ResetColor();
            } 
            else 
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Game over, you lose to {Battleships.OpponentName}!" + 
                    $"\nThanks for playing, {Battleships.PlayerName}.");
                Console.ResetColor();
            }
            Battleships.Shutdown();
        }


        public static void YourTurn()
            // logic for when this instance is firing a shot
        {
            bool validInput = false;
            string shot = "";

            int column = -1; // declare column+row before loop; -1 represents invalid
            int row = -1;

            while (!validInput) // receive user input
            {
                Console.Write("Enter the coordinates of the cell you would like to shoot at:\n\t>");
                shot = Console.ReadLine()!;

                    // -- validate input --
                // 1. check for null
                if (shot is null) continue;

                // 2. check for END command
                shot = shot.ToUpper().Replace(" ", "").Replace("\t", ""); // cleanup string
                if (shot == "END" || shot == "'END'")
                {
                    Console.WriteLine("Game is being cancelled...");
                    OpponentConnection.Send("END");
                    StillPlaying = false;
                    return;
                }

                // 3. regex validation
                if (!ShootingRegex.IsMatch(shot))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Invalid input! Enter a column [A-J] followed by a row [1-10] like this:");
                    Console.WriteLine("\t>A1");
                    Console.ResetColor();
                    continue;
                }

                // 4. duplicate shot check
                column = CharTransform.ColumnNo(shot[0]);
                row = int.Parse(shot[1..].ToString()) - 1;
                if (gg.IsValidTarget(column, row)) validInput = true;
                else 
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("You have already fired at this square! Try another target.");
                    Console.ResetColor();
                } 
            }

            // send message
            string response = OpponentConnection.FireAtOpponent(column, row);
            Console.WriteLine($"You fired at opponent: {response}\n");
        }

        public static void OpponentsTurn()
            // logic for when opponent is firing a shot
        {
            Console.WriteLine($"Waiting for {Battleships.OpponentName} to shoot...");
            string result = OpponentConnection.OpponentFiresAtUs();
            Console.WriteLine($"Opponent fired at us: {result}");
        }

        public static void PlaceShipsRandomly()
            // randomly place your ships on the board
        {
            gg = new(); // useful for when testing iterations of random placement
            Random rand = new(); // we reuse the same Random object for efficiency

            // Placing Aircraft Carrier
            PlaceOneShipRandomly('A', rand);
            // Placing Battleship
            PlaceOneShipRandomly('B', rand);
            // Placing Cruiser
            PlaceOneShipRandomly('C', rand);
            // Placing Patrol Boat
            PlaceOneShipRandomly('P', rand);
            // Placing Submarine
            PlaceOneShipRandomly('S', rand);

            // print ship placement when finished
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(gg.ToString(true));
            Console.ResetColor();
        }

        private static void PlaceOneShipRandomly(char type, Random rand)
            // random placement for single ship
        { 
            bool isHorizontal, positionFound;
            int row, column, size;

            do
            {
                isHorizontal = (rand.Next(2) == 1);
                size = CharTransform.ShipSize(type);
                positionFound = true; // easier to default to true and change if not

                // set random coordinates
                if (isHorizontal) // valid positions are different for horizontal and vertical
                {
                    row = rand.Next(10);
                    column = rand.Next(10 - size);
                }
                else
                {
                    column = rand.Next(10);
                    row = rand.Next(10 - size);
                }

                // check if random coordinates are valid
                for (int i = 0; i < size; i++)
                {
                    if (isHorizontal) // we need to split horiz/vert because they iterate cells differently
                    {
                        if (gg.GetCell(column + i, row, true) != ' ')
                        {
                            positionFound = false;
                        }
                    }
                    else // if placing vertically
                    {
                        if (gg.GetCell(column, row + i, true) != ' ')
                        {
                            positionFound = false;
                        }
                    }
                }
            } while (!positionFound); // if placement is not valid, select new random position

            // store position in GameGrids object
            gg.PlaceShip(column, row, isHorizontal, type);
        }

        public static void PlaceShipsManually()
            // not required by spec but I thought this was fun
        {
            gg = new(); // useful for when testing iterations of random placement

            // info display
            string info1 = $"======= Placing ships : {Battleships.PlayerName} =======";
            string info2 = new string('=', info1.Length);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n\n{info2}\n{info1}\n{info2}\n");
            Console.ResetColor();

            Console.WriteLine("First, you must place your ships on your grid.\n" +
                "Ships are placed by entering the cell in the top left of the desired" +
                "position, and can be placed either horizontally (H) or vertically (V).\n" +
                "An example input would be A1H, where the ship would be added horizontally, " +
                "starting from cell A1 and going to the right.\n" +
                "Ships cannot be placed on top of each other.\n");

            Console.WriteLine("Placing first ship - Aircraft Carrier, 5 blocks long.");
            PlaceOneShip('A');
            Console.WriteLine("Placing second ship - Battleship, 4 blocks long.");
            PlaceOneShip('B');
            Console.WriteLine("Placing third ship - Cruiser, 3 blocks long.");
            PlaceOneShip('C');
            Console.WriteLine("Placing fourth ship - Patrol Boat, 2 blocks long.");
            PlaceOneShip('P');
            Console.WriteLine("Placing fifth and final ship - Submarine, 3 blocks long.");
            PlaceOneShip('S');
        }

        public static void PlaceOneShip(char type)
        {
            string pos = "";
            bool validInput = false;
            int size = CharTransform.ShipSize(type);

            int column = -1; // declare column+row before loop; -1 represents invalid
            int row = -1;
            bool isHorizontal = true; // horizontal or vertical placement

            while (!validInput)
            {
                // receive input
                Console.Write("Enter the desired position:\n\t>");
                pos = Console.ReadLine()!;

                    // -- validate input --

                // 1. check for null
                if (pos is null) continue;

                // 2. regex validation
                pos = pos.ToUpper().Replace(" ", "").Replace("\t", "");
                if (!PlacementRegex.IsMatch(pos))
                {
                    Console.WriteLine("Invalid input! Enter a column [A-J] followed by a row [1-10]\n" +
                        "and either a H or V like this::");
                    Console.WriteLine("\t>A1H");
                    continue;
                }

                // 3. position clash check
                column = CharTransform.ColumnNo(pos[0]);
                row = int.Parse(pos[1..^1].ToString()) - 1;
                isHorizontal = (pos[^1] == 'H');

                bool positionError = false;
                for (int i = 0; i < size; i++)
                {
                    if (isHorizontal) // if placing horizontally
                    {
                        if (gg.GetCell(column + i, row, true) != ' ')
                        {
                            Console.WriteLine($"Invalid input! This placement clashes with your " +
                                $"{CharTransform.ShipType(gg.GetCell(column + i, row, true))}");
                            positionError = true;
                            break;
                        }
                    }
                    else // if placing vertically
                    {
                        if (gg.GetCell(column, row + i, true) != ' ')
                        {
                            Console.WriteLine($"Invalid input! This placement clashes with your " +
                                $"{CharTransform.ShipType(gg.GetCell(column, row + i, true))}");
                            positionError = true;
                            break;
                        }
                    }
                }

                if (!positionError) validInput = true;
            }

            // store position in GameGrids object
            gg.PlaceShip(column, row, isHorizontal, type);

            // display placement of ships on console
            Console.WriteLine($"{CharTransform.ShipType(type)} placed successfully!\n\n" +
                $"{gg.ToString(true)}");
        }
    }
}