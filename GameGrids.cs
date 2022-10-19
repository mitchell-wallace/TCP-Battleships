using System.Text;

namespace Battleships
{

    // firingGrid char meanings:
    //      ' ' = unknown
    //      'X' = hit
    //      '~' = miss

    // homeGrid char meanings:
    //      ' ' = empty
    //      '~' = missed shot by opponent
    //      'A' = Aircraft carrier (5 blocks)
    //      'B' = Battleship (4 blocks)
    //      'C' = Cruiser (3 blocks)
    //      'P' = Patrol boat (2 blocks)
    //      'S' = Submarine (3 blocks)
    //      lowercase indicates a hit
    //      'X' = A sunken ship

    // input validation is all to be handled in UserInterface

    internal class GameGrids
    {

        private char[,] firingGrid = new char[10, 10]; // grid marking your history of firing at the opponent's ships
        private char[,] homeGrid = new char[10, 10]; // grid marking the position of your own ships

        public GameGrids()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    firingGrid[i, j] = ' ';
                    homeGrid[i, j] = ' ';
                }
            }
        }

        public void Reset()
        {
            firingGrid = new char[10, 10];
            homeGrid = new char[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    firingGrid[i, j] = ' ';
                    homeGrid[i, j] = ' ';
                }
            }
        }

        public void FireShot(int column, int row, bool success)
        { // mark the result of a shot against your opponent
            if (success) firingGrid[column, row] = 'X';
            else firingGrid[column, row] = '~';
        }

        public bool IsValidTarget(int column, int row)
        {
            if (firingGrid[column, row] != ' ')
            {
                return false;
            }
            return true;
        }

        public bool ReceiveShot(int column, int row) // return: true is HIT, false is MISS
        { // use this method when your opponent is shooting at your ships

            // check if hit and store result
            if (homeGrid[column, row] == ' ')
            {
                homeGrid[column, row] = '~'; // mark as missed shot by opponent
                return false;
            }
            else
            {
                // homeGrid[column, row] = homeGrid[column, row].ToString().ToLower()[0];
                homeGrid[column, row] = 'X';
                return true;
            }
        }

        public void PlaceShip(int column, int row, bool isHorizontal, char type)
        {
            int size = CharTransform.ShipSize(type);

            for (int i = 0; i < size; i++)
            {
                if (isHorizontal)
                    homeGrid[column + i, row] = type;
                else
                    homeGrid[column, row + i] = type;
            }
        }

        public char GetCell(int column, int row, bool isHomeGrid = true)
        {
            if (isHomeGrid) return homeGrid[column, row];
            else return firingGrid[column, row];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("   ~~~ Y O U R    S H I P S ~~~            ~~~  OPPONENT'S SHIPS  ~~~\n");
            sb.Append("   A  B  C  D  E  F  G  H  I  J           A  B  C  D  E  F  G  H  I  J\n");
            for (int i = 0; i < 10; i++)
            {

                sb.Append((i + 1) + " ");
                if (i != 9) sb.Append(" ");

                for (int j = 0; j < 10; j++)
                {
                    sb.Append(homeGrid[j, i] + "  ");
                }

                sb.Append("      " + (i + 1) + " ");
                if (i != 9) sb.Append(" ");

                for (int j = 0; j < 10; j++)
                {
                    sb.Append(firingGrid[j, i] + "  ");
                }

                sb.Append("\n");
            }
            return sb.ToString();
        }

        public string ToString(bool isHomeGrid) // for displaying only a single grid
        {
            StringBuilder sb = new StringBuilder();

            if (isHomeGrid)
            {
                sb.Append("   ~~~ Y O U R    S H I P S ~~~\n");
                sb.Append("   A  B  C  D  E  F  G  H  I  J\n");
                for (int i = 0; i < 10; i++)
                {

                    sb.Append((i + 1) + " ");
                    if (i != 9) sb.Append(" ");

                    for (int j = 0; j < 10; j++)
                    {
                        sb.Append(homeGrid[j, i] + "  ");
                    }

                    sb.Append("\n");
                }
            }
            else
            {
                sb.Append("    ~~~  OPPONENT'S SHIPS  ~~~\n");
                sb.Append("   A  B  C  D  E  F  G  H  I  J\n");
                for (int i = 0; i < 10; i++)
                {

                    sb.Append("      " + (i + 1) + " ");
                    if (i != 9) sb.Append(" ");

                    for (int j = 0; j < 10; j++)
                    {
                        sb.Append(firingGrid[j, i] + "  ");
                    }

                    sb.Append("\n");
                }
            }

            return sb.ToString();
        }
    }
}