namespace Battleships {

    public class UserInterface {

        public static void Play() {
            bool StillPlaying = true;
            GameGrids gg = new GameGrids();

            Console.WriteLine("You may send the message 'END' at any time to end the game.");

            while (StillPlaying) {
                Console.Write("Enter the coordinates of the square you would like to shoot at:\n\t>");
                string shot = Console.ReadLine()!;

                if (shot is null) continue;
                if (shot == "END" || shot == "'END'") {
                    StillPlaying = false;
                    continue;
                }
                if (shot.Length != 2) continue; // TODO: validate shots against regex

                if (gg.FireShot(shot[0],shot[1])) Console.WriteLine("You HIT an enemy ship!");
                else Console.WriteLine("You MISSED!");

                Console.WriteLine();
                Console.WriteLine(gg.ToString());
                 
            }

            Console.WriteLine("Game has ended. Thanks for playing!");

        }

        

    }
}