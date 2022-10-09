namespace Battleships
{
    internal class CharTransform
    {
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

        public static string ShipType(char type)
        {
            switch (type)
            {
                case 'A': return "Aircraft Carrier";
                case 'B': return "Battleship";
                case 'C': return "Cruiser";
                case 'P': return "Patrol Boat";
                case 'S': return "Submarine";
                default: return "<unknown ship>";
            }
        }
        
        public static int ShipSize(char type)
        {
            switch (type)
            {
                case 'A': return 5;
                case 'B': return 4;
                case 'C': return 3;
                case 'P': return 2;
                case 'S': return 3;
                default: return -1;
            }
        }
    }
}
