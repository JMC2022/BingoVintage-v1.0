using BingoVintage.Data;

namespace BingoVintage.ExtentionMethods
{
    public class BingoBallMethod
    {
        public int RandomBall()
        {
            int rnd = new Random().Next(1, 91);
            return rnd;
        }

        public bool IsInHstory(int[] balls, int rnd)
        {
            bool compare = false;
            foreach (var item in balls)
            {
                compare = item.Equals(rnd);
                if (compare)
                {
                    break;
                }
            }
            return compare;
        }
    }
}
