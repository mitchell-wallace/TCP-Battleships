namespace Battleships{

    public class OpponentConnection{ // TODO: implement actual connection to opponent

        private static int counter = 0;

        public static bool FireAtOpponent(int column, int row) { // TODO: implement ship placement for dummy opponent
            if (column == 2) return true;
            return false;
        }

        public static int[] OpponentFiresAtUs()
        {
            int[] result = { counter % 10, counter / 10};
            counter++;
            return result;
        }

        public static void ResponseToOpponentShot(string response)
        {
            // TODO: respond to opponent with MISS, HIT, SUNK messages
        }

    }
}