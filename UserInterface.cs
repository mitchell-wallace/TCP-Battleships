using System.Text.RegularExpressions;

namespace Battleships {

    public class UserInterface {

        static bool StillPlaying = true; // loop break variable
        static string whoseTurn = ""; // solely for display
        static int turnNo = 1; // primarily for display; simpler to start from 1
        static GameGrids gg = new GameGrids(); // data structure for game data

        static Regex rg = new Regex(@"^[A-J][1-9]0?$"); // regex pattern for validating input

        public static void Play(bool isPlayer1) { // isPlayer1 determines whether odd-numbered turns or even-numbered turns are our turn to shoot
            Console.WriteLine("You may send the message 'END' at any time to end the game.\n");

            // static variable re-initialisation
            StillPlaying = true;
            turnNo = 1;

            // setup ship placement
            PlaceShips();

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

        public static void PlaceShips() // TODO: implement ship placement in UI
        {
            Console.WriteLine("You cannot yet manually place ships!");
        }

        public static void YourTurn()
        {
            bool validInput = false;
            string shot = "";

            int column = -1; // declare column+row before loop
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
                if (!rg.IsMatch(shot))
                {
                    Console.WriteLine("Invalid input! Enter a column [A-J] followed by a row [1-10] like this:");
                    Console.WriteLine("\t>A1");
                    continue;
                }

                // duplicate shot check
                column = ColumnNo(shot[0]) - 1;
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

            Console.WriteLine($"Opponent fired at {ColumnChar(shot[0]+1)}{shot[1]+1} and {result}!");

        }

        public static int ColumnNo(char column)
        {
            switch (column)
            {
                case 'A':
                    return 1;
                case 'B':
                    return 2;
                case 'C':
                    return 3;
                case 'D':
                    return 4;
                case 'E':
                    return 5;
                case 'F':
                    return 6;
                case 'G':
                    return 7;
                case 'H':
                    return 8;
                case 'I':
                    return 9;
                case 'J':
                    return 10;
                default: return -1; // -1 indicates invalid
            }
        }

        public static char ColumnChar(int column)
        {
            switch (column)
            {
                case 1:
                    return 'A';
                case 2:
                    return 'B';
                case 3:
                    return 'C';
                case 4:
                    return 'D';
                case 5:
                    return 'E';
                case 6:
                    return 'F';
                case 7:
                    return 'G';
                case 8:
                    return 'H';
                case 9:
                    return 'I';
                case 10:
                    return 'J';
                default: return '?'; // -1 indicates invalid
            }
        }



    }
}