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
        private int aircraftCarrierCells = 5, battleshipCells = 4, cruiserCells = 3, patrolBoatCells = 2, submarineCells = 3;

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

        public char ReceiveShot(int column, int row) // return: 'X' is HIT, '~' is MISS,
            // uppercase letter is sunk on ship matching that char,
            // lowershase letter is sunk and game over
            // use this method when your opponent is shooting at your ships
        {
            char result = ' '; // check if hit and store result
            if (homeGrid[column, row] == ' ')
            {
                homeGrid[column, row] = '~'; // mark as missed shot by opponent
                result = '~';
            }
            else
            {
                result = 'X'; // ovverridden if sunk
                char k = homeGrid[column, row];
                switch (k)
                {
                    case 'A':
                        aircraftCarrierCells--; // decrement the number of cells remaining for this ship
                        if (aircraftCarrierCells == 0)
                        { // 0 indicates sunk
                            result = k;
                            aircraftCarrierCells--; // decrement to -1 to indicate sunk logic has been processed
                        }
                        break;
                    case 'B':
                        battleshipCells--;
                        if (battleshipCells == 0)
                        {
                            result = k;
                            battleshipCells--;
                        }
                        break;
                    case 'C':
                        cruiserCells--;
                        if (cruiserCells == 0)
                        {
                            result = k;
                            cruiserCells--;
                        }
                        break;
                    case 'P':
                        patrolBoatCells--;
                        if (patrolBoatCells == 0)
                        {
                            result = k;
                            patrolBoatCells--;
                        }
                        break;
                    case 'S':
                        submarineCells--;
                        if (submarineCells == 0)
                        {
                            result = k;
                            submarineCells--;
                        }
                        break;
                    default: break;
                }
                homeGrid[column, row] = 'X';
            }

            if (aircraftCarrierCells + battleshipCells + cruiserCells + patrolBoatCells + submarineCells == -5) 
                // if all five indicate -5, then all 5 have been sunk, and we send "GAME OVER"
                // we also need to indicate which ship was sunk, so we do this with lowercase to keep to a single character
            {
                result = result.ToString().ToLower()[0];
            }

            return result;
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
            // returns both game grids as a string for display
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

        public string ToString(bool isHomeGrid) 
            // overload for displaying only a single grid
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