namespace Battleships
{
    internal class CharTransform
    {
        public static int ColumnNo(char column)
            // convert column letter to column index
        {
            switch (column)
            {
                case 'A': return 0;
                case 'B': return 1;
                case 'C': return 2;
                case 'D': return 3;
                case 'E': return 4;
                case 'F': return 5;
                case 'G': return 6;
                case 'H': return 7;
                case 'I': return 8;
                case 'J': return 9;
                default: return -1; // -1 indicates invalid
            }
        }

        public static char ColumnChar(int column)
            // convert column index to column letter
        {
            switch (column)
            {
                case 0: return 'A';
                case 1: return 'B';
                case 2: return 'C';
                case 3: return 'D';
                case 4: return 'E';
                case 5: return 'F';
                case 6: return 'G';
                case 7: return 'H';
                case 8: return 'I';
                case 9: return 'J';
                default: return '?';
            }
        }

        public static string ShipType(char type)
            // convert ship type char to ship type name
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
            // convert ship type char to ship size
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
