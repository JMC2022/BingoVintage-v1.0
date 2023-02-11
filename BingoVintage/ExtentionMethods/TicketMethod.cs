using BingoVintage.Models;
using System.Net;

namespace BingoVintage.ExtentionMethods
{
    public class TicketMethod
    {
        // Random Unique Identificator for tickets.
        public int Identificator(byte length)
        {
            string Identificator = "";
            for (byte i = 0; i < length; i++)
            {
                char t = GetChar();
                Identificator += t.ToString();
            }
            //Format string to int, no exception because the string are numbers. 
            var Ident = int.Parse(Identificator);
            return Ident;
        }
        private static char GetChar() 
        {
            string chars = "0123456789";
            int randomChar = new Random().Next(0, chars.Length);
            return chars[randomChar];
        }

        // 27 Natural Random Numbers for ticket.
        public List<Numbers> Numbers()
        {
            // Initial Variables.
            var listNumber = new List<int>();
            int[] minVal = {1, 10, 20, 30, 40, 50, 60, 70, 80 };
            int[] maxVal = {10, 20, 30, 40, 50, 60, 70, 80, 91 };
            int o = 0;
            int min = minVal[0];
            int max = maxVal[0];

            for (int i = 0; i < 27; i++)
            {
                // Random number Generator.
                if (i == 3 || i == 6 || i == 9 || i == 12 || i == 15 || i == 18 || i == 21 || i == 24)
                {
                    o++;
                    min = minVal[o];
                    max = maxVal[o];
                }
                int rnd = new Random().Next(min, max);

                // Don`t repeat numbers
                bool contains = listNumber.Contains(rnd);
                if (!contains) { listNumber.Add(rnd);}
                else{i--;}
            }

            // Order the List ascending
            var finishList = listNumber.OrderBy(x => x).ToList();

            //Switch random for empty spaces
            int swtch = new Random().Next(1, 5);

            // Patterns for empty spaces
            int[] ptrn = new int[12];
            switch (swtch) 
            {
                case 1:
                    ptrn = new int[] {2,4,6,11,12,13,17,18,19,23,24,25};
                    break;
                case 2:
                    ptrn = new int[] {0,2,4,6,11,12,13,17,19,21,22,26};
                    break;
                case 3:
                    ptrn = new int[] {2,3,7,9,11,12,16,19,20,23,24,25};
                    break;
                case 4:
                    ptrn = new int[] {0,5,8,10,12,14,15,18,19,22,23,25};
                    break;
            }

            // Make the List optimal to play and organice all numbers and spaces.
            for (int i = 0; i < 27; i++)
            {
                bool contain = ptrn.Contains(i);
                if (contain)
                {
                    finishList.RemoveAt(i);
                    finishList.Insert(i, 0);
                }
            }
            var lNumFinish = new List<Numbers> { };
            for (int i = 0; i < finishList.Count; i++)
            {
                lNumFinish.Add(new Numbers() { Num = finishList[i] });
            }
            return lNumFinish;
        }
    }
}