using System.Text;

namespace Battleships {

    // character meanings:
    //      ' ' = unknown
    //      'X' = hit
    //      '~' = miss

    public class GameGrids {

        char[,] firingGrid = new char[10, 10]; // grid marking your history of firing at the opponent's ships
        char[,] homeGrid = new char[10, 10]; // grid marking the position of your own ships

        public GameGrids() {
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < 10; j++) {
                    firingGrid[i, j] = ' ';
                    homeGrid[i, j] = ' ';
                }
            }
        }


        public bool FireShot(char column, int row) { // use this method when you are shooting at your opponent's ships
            int columnNo = ColumnNo(column);

            // check if valid square
            if (columnNo == -1 || row > 10 || row < 1) {
                Console.WriteLine("ERROR: Invalid cell address!"); // TODO: decide on a way to re-prompt for a new shot
                return false;
            }
            if (firingGrid[columnNo, row] != ' ') {
                Console.WriteLine("ERROR: Cell has already been tried!");
                return false;
            } 

            // check if hit and store result
            if (OpponentConnection.TryHit(columnNo, row)) {
                firingGrid[columnNo, row] = 'X';
                return true;
            } 
            else {
                firingGrid[columnNo, row] = '~';
                return false;
            }
        }

        public bool ReceiveShot(char column, int row) { // use this method when your opponent is shooting at your ships
            int columnNo = ColumnNo(column);

            // check if valid square
            if (columnNo == -1 || row > 10 || row < 1) {
                Console.WriteLine("ERROR: Invalid cell address!");
                return false;
            }

            // check if hit and store result
            if (firingGrid[columnNo, row] == ' ') {
                return false;
            } 
            else {
                return true;
            }
        }

        public override string ToString() { // TODO: also display own grid -- done?
            StringBuilder sb = new StringBuilder();
            sb.Append("   ~~~ Y O U R    S H I P S ~~~            ~~~  OPPONENT'S SHIPS  ~~~\n");
            sb.Append("   A  B  C  D  E  F  G  H  I  J           A  B  C  D  E  F  G  H  I  J\n");
            for (int i = 0; i < 10; i++) {

                sb.Append((i+1) + " ");
                if (i != 10) sb.Append(" ");
                
                for (int j = 0; j < 10; j++) {
                    sb.Append(homeGrid[i, j] + "  ");
                }

                sb.Append("      " + (i+1) + " ");
                if (i != 10) sb.Append(" ");

                for (int j = 0; j < 10; j++) {
                    sb.Append(firingGrid[i, j] + "  ");
                }

                sb.Append("\n");
            }
            return sb.ToString();
        }

        public int ColumnNo(char column) {
            switch (column) {
                case 'a': case 'A':
                    return 1;
                case 'b': case 'B':
                    return 2;
                case 'c': case 'C':
                    return 3;
                case 'd': case 'D':
                    return 4;
                case 'e': case 'E':
                    return 5;
                case 'f': case 'F':
                    return 6;
                case 'g': case 'G':
                    return 7;
                case 'h': case 'H':
                    return 8;
                case 'i': case 'I':
                    return 9;
                case 'j': case 'J':
                    return 10;
                default: return -1;
            }
        }

    }
}