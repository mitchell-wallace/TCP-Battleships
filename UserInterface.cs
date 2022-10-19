using System.Data.Common;
using System.Text.RegularExpressions;

namespace Battleships {

    internal class UserInterface {

        static bool StillPlaying = true; // loop break variable
        static string whoseTurn = ""; // solely for display
        static int turnNo = 1; // primarily for display; simpler to start from 1
        static GameGrids gg = new GameGrids(); // data structure for game data
        static Regex ShootingRegex = new Regex(@"^[A-J][1-9]0?$"); // regex pattern for validating shooting input
        static Regex PlacementRegex = new Regex(@"^[A-J](10|[1-9])$"); // regex pattern for validating ship placement input

        public static void Play(bool isPlayer1) { // isPlayer1 determines whether odd-numbered turns or even-numbered turns are our turn to shoot
            Console.WriteLine("You may send the message 'END' at any time to end the game.");

            // static variable re-initialisation
            StillPlaying = true;
            turnNo = 1;

            // setup ship placement
            // PlaceShipsManually();
            PlaceShipsRandomly();

            while (StillPlaying) {
                // logic for alternating turns
                bool yourTurn = (isPlayer1 && turnNo % 2 == 1 || !isPlayer1 && turnNo % 2 == 0);
                if (yourTurn) whoseTurn = "YOUR turn";
                else whoseTurn = "OPPONENT'S turn";

                // info display
                string info1 = $"======= Turn number {turnNo} : {whoseTurn} =======";
                string info2 = new string('=', info1.Length);
                Console.WriteLine($"\n\n{info2}\n{info1}\n{info2}\n");

                // call code for the turn
                if (yourTurn) YourTurn();
                else OpponentsTurn();

                // end of turn process
                Console.WriteLine("\n" + gg.ToString());
                turnNo++;
            }
            Console.WriteLine("Game has ended. Thanks for playing!");
        }



        public static void YourTurn()
        {
            bool validInput = false;
            string shot = "";

            int column = -1; // declare column+row before loop; -1 represents invalid
            int row = -1;

            while (!validInput) // receive user input
            {
                Console.Write("Enter the coordinates of the square you would like to shoot at:\n\t>");
                shot = Console.ReadLine()!;

                // validate input

                // check for null
                if (shot is null) continue;

                // check for END command
                shot = shot.ToUpper().Replace(" ","").Replace("\t",""); // cleanup string
                if (shot == "END" || shot == "'END'")
                {
                    Console.WriteLine("Game is being cancelled..."); // TODO: send message to opponent?
                    StillPlaying = false;
                    return;
                }

                // regex validation
                if (!ShootingRegex.IsMatch(shot))
                {
                    Console.WriteLine("Invalid input! Enter a column [A-J] followed by a row [1-10] like this:");
                    Console.WriteLine("\t>A1");
                    continue;
                }

                // duplicate shot check
                column = CharTransform.ColumnNo(shot[0]) - 1;
                row = int.Parse(shot[1..].ToString()) - 1;
                if (gg.IsValidTarget(column, row)) validInput = true;
                else Console.WriteLine("You have already fired at this square! Try another target.");

            }

            // parse input


            // fire shot and display result
            switch (gg.FireShot(column, row))
            {
                case -1: Console.WriteLine("You MISSED!"); break;
                case 1: Console.WriteLine("You HIT an enemy ship!"); break;
                case 0: YourTurn(); break; // error result
                default: break;
            }

            Console.WriteLine();
        }

        public static void OpponentsTurn()
        {
            int[] shot = OpponentConnection.OpponentFiresAtUs();
            string result = ""; // TODO: handle SUNK messages
            if (gg.ReceiveShot(shot)) result = "HIT";
            else result = "MISSED";

            OpponentConnection.ResponseToOpponentShot(result);

            Console.WriteLine($"Opponent fired at {CharTransform.ColumnChar(shot[0]+1)}{shot[1]+1} and {result}!");
        }

        public static void PlaceShipsRandomly() {
            // TODO: improve this. It's pretty low-IQ currently.
            // we randomly choose horizontal or vertical, and all ships will face that direction
            // we pick random rows or columns respectively, where all 10 are available,
            // then set a random offset for the long side of the ship
            // this is a pretty bad explanation but I can't call them rows or columns when that part is randomised...

            Random rand = new();
            int mainCoord = -1;
            int offset = -1;

            // for (int i = 0; i < 20; i++)
            // {
            //     bool b = (rand.Next(2) == 1);
            //     Console.WriteLine($"{i}: {b}");
            // }
            bool isHorizontal = (rand.Next(2) == 1);
            
            List<int> placesUsed = new();

            // place aircraft carrier
            do {
                mainCoord = rand.Next(10);
            } while (placesUsed.Contains(mainCoord)); // if main coord already used, roll again
            placesUsed.Add(mainCoord);
            offset = rand.Next(5);
            if (isHorizontal) gg.PlaceShip(offset, mainCoord, isHorizontal, 'A');
            else gg.PlaceShip(mainCoord, offset, isHorizontal, 'A');

            // place battleship
            do {
                mainCoord = rand.Next(10);
            } while (placesUsed.Contains(mainCoord)); // if main coord already used, roll again
            placesUsed.Add(mainCoord);
            offset = rand.Next(6);
            if (isHorizontal) gg.PlaceShip(offset, mainCoord, isHorizontal, 'B');
            else gg.PlaceShip(mainCoord, offset, isHorizontal, 'B');

            // place cruiser
            do {
                mainCoord = rand.Next(10);
            } while (placesUsed.Contains(mainCoord)); // if main coord already used, roll again
            placesUsed.Add(mainCoord);
            offset = rand.Next(7);
            if (isHorizontal) gg.PlaceShip(offset, mainCoord, isHorizontal, 'C');
            else gg.PlaceShip(mainCoord, offset, isHorizontal, 'C');

            // place patrol boat
            do {
                mainCoord = rand.Next(10);
            } while (placesUsed.Contains(mainCoord)); // if main coord already used, roll again
            placesUsed.Add(mainCoord);
            offset = rand.Next(8);
            if (isHorizontal) gg.PlaceShip(offset, mainCoord, isHorizontal, 'P');
            else gg.PlaceShip(mainCoord, offset, isHorizontal, 'P');

            // place submarine
            do {
                mainCoord = rand.Next(10);
            } while (placesUsed.Contains(mainCoord)); // if main coord already used, roll again
            placesUsed.Add(mainCoord);
            offset = rand.Next(7);
            if (isHorizontal) gg.PlaceShip(offset, mainCoord, isHorizontal, 'S');
            else gg.PlaceShip(mainCoord, offset, isHorizontal, 'S');
            
            Console.WriteLine(gg.ToString(true));
        }

        public static void PlaceShipsManually(bool silent = false) // TODO: the spec requires random placement
        {
            //Console.WriteLine("You cannot yet manually place ships!");

            // info display
            string info1 = $"======= Placing ships : {Battleships.PlayerName} =======";
            string info2 = new string('=', info1.Length);
            if (!silent) Console.WriteLine($"\n\n{info2}\n{info1}\n{info2}\n");

            if (!silent) Console.WriteLine("First, you must place your ships on your grid.\n" +
                "Ships are placed by entering the cell in the top left of the desired" +
                "position, and can be placed either horizontally (H) or vertically (V).\n" +
                "An example input would be A1H, where the ship would be added horizontally, " +
                "starting from cell A1 and going to the right.\n" +
                "Ships cannot be placed on top of each other.\n");

            if (!silent) Console.WriteLine("Placing first ship - Aircraft Carrier, 5 blocks long.");
            PlaceOneShip('A');
            if (!silent) Console.WriteLine("Placing second ship - Battleship, 4 blocks long.");
            PlaceOneShip('B');
            if (!silent) Console.WriteLine("Placing third ship - Cruiser, 3 blocks long.");
            PlaceOneShip('C');
            if (!silent) Console.WriteLine("Placing fourth ship - Patrol Boat, 2 blocks long.");
            PlaceOneShip('P');
            if (!silent) Console.WriteLine("Placing fifth and final ship - Submarine, 3 blocks long.");
            PlaceOneShip('S');
        }

        public static void PlaceOneShip(char type, bool silent = false)
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
                if (!silent) Console.Write("Enter the desired position:\n\t>");
                pos = Console.ReadLine()!;

                // validate input

                // check for null
                if (pos is null) continue;

                // regex validation
                pos = pos.ToUpper().Replace(" ", "").Replace("\t", "");
                if (!PlacementRegex.IsMatch(pos))
                {
                    if (!silent) Console.WriteLine("Invalid input! Enter a column [A-J] followed by a row [1-10]\n" +
                        "and either a H or V like this::");
                    if (!silent) Console.WriteLine("\t>A1H");
                    continue;
                }

                // position clash check
                column = CharTransform.ColumnNo(pos[0]) - 1;
                row = int.Parse(pos[1..^1].ToString()) - 1;
                isHorizontal = (pos[^1] == 'H');

                bool positionError = false;
                for (int i = 0; i < size; i++)
                {
                    if (isHorizontal) // if placing horizontally
                    {
                        if (gg.GetCell(column + i, row, true) != ' ')
                        {
                            if (!silent) Console.WriteLine($"Invalid input! This placement clashes with your " +
                                $"{CharTransform.ShipType(gg.GetCell(column + i, row, true))}");
                            positionError = true;
                            break;
                        }
                    }
                    else // if placing vertically
                    {
                        if (gg.GetCell(column, row + i, true) != ' ')
                        {
                            if (!silent) Console.WriteLine($"Invalid input! This placement clashes with your " +
                                $"{CharTransform.ShipType(gg.GetCell(column, row + i, true))}");
                            positionError = true;
                            break;
                        }
                    }
                }

                if (!positionError) validInput = true;
            }

            gg.PlaceShip(column, row, isHorizontal, type);
            if (!silent) Console.WriteLine($"{CharTransform.ShipType(type)} placed successfully!\n\n" +
                $"{gg.ToString(true)}");
        }
    }
}