using System;
namespace CSMud
{
    public static class Dice
    {
        public static int RollTwo()
        {
            Random rand = new Random();
            return rand.Next(3);
        }
        
        public static int RollFour()
        {
            Random rand = new Random();
            return rand.Next(5);
        }

        public static int RollEight()
        {
            Random rand = new Random();
            return rand.Next(9);
        }
        
        public static int RollTen()
        {
            Random rand = new Random();
            return rand.Next(11);
        }

        public static int RollTwelve()
        {
            Random rand = new Random();
            return rand.Next(13);
        }

        public static int RollTwenty()
        {
            Random rand = new Random();
            return rand.Next(21);
        }

        public static int RollHundred()
        {
            Random rand = new Random();
            return rand.Next(101);
        }
    }
}
