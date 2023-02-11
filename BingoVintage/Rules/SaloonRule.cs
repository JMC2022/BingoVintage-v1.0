using BingoVintage.Data;
using BingoVintage.ExtentionMethods;
using BingoVintage.Models;
using System.Security.Claims;

namespace BingoVintage.Rules
{
    public class SaloonRule
    {
        private readonly List<Ticket> _listOfTicket = new();

        // Rule for New Temporary Tickets
        public IEnumerable<Ticket> CreateTempTicket(int cantT, ClaimsPrincipal claim)
        {
            // Verify Claims cookie with DB - Sanity check!
            int idCookie = new UsrMethod().CompareIdUsrCookieToDB(claim);

            //Delete all tmp data from DB and start over.
            var data = new SaloonData();
            data.DeleteTmpTicket();

            //Create Tmp ticket on DB and return list of ticket to View.
            for (int i = 0; i < cantT; i++)
            {
                int identificator = new TicketMethod().Identificator(6);
                List<Numbers> listNumbers = new TicketMethod().Numbers();
                _listOfTicket.Add(
                    new Ticket()
                    {
                        TicketNo = i + 1,
                        UsrID = idCookie,
                        Identificator = identificator,
                        Numbers = listNumbers,
                        Playing = false
                    });
                data.CreateTmpTicket(_listOfTicket[i]);
            }
            return _listOfTicket;
        }

        // Save Playing Tickets into Db
        public void SaveTktIntoDB()
        {
            var data = new SaloonData();
            data.LoadTicketToDb();
        }

        //Get List of Playing Tickets From Db - v1.0
        public List<Ticket> GetListOfTickets(int usrId)
        {
            var ListT = new SaloonData().GetActiveListOfTickets(usrId);
            for (int i = 0; i < ListT.Count; i++)
            {
                //List<int> listNumbers = new SaloonData().GetListOfNumbersFromDB(ListT[i].TicketID);
                List<Numbers> listNumbers = new SaloonData().GetActiveListOfNumbers(ListT[i].TicketID);
                ListT[i].Numbers = listNumbers;
            }
            return ListT;
        }

        public void DisableT(ClaimsPrincipal c)
        {
            var usrId = new UsrMethod().CompareIdUsrCookieToDB(c);
            if (usrId != 0)
                new SaloonData().DisablePlayingTickets(usrId);
        }

        public BingoBallHistory LaunchBall(ClaimsPrincipal c)
        {
            bool isInHistory;
            int[] balls = new SaloonData().GetBallsPlaying();
            int rnd;
            do
            {
                rnd = new BingoBallMethod().RandomBall();
                isInHistory = new BingoBallMethod().IsInHstory(balls, rnd);
                if (!isInHistory)
                {
                    var usrId = new UsrMethod().CompareIdUsrCookieToDB(c);
                    new SaloonData().CreateHistoryForUsr(rnd, usrId);
                }
            } while (isInHistory);
            return new BingoBallHistory() { BallNo = rnd };
        }

        // Get number of playing tickets.
        public int GetCantT(ClaimsPrincipal c)
        {
            int usrId = new UsrMethod().CompareIdUsrCookieToDB(c);
            var cantT = new SaloonData().GetCantPlayingTicket(usrId);
            return cantT;
        }

        //Check if Ticket match an acert and return new array with int properties.
        private static Array CheckAcert(int UsrId)
        {
            var data = new SaloonData();
            int numBall = data.GetLastBallPlayed(UsrId);
            bool b = data.IsAcert(UsrId, numBall);
            int parsedB;
            if (b)
            { parsedB = 1; }
            else
            { parsedB = 0; }
            var acert = new[] { parsedB, numBall };
            return acert;
        }

        //Return a List with acert and index values, for jQuery.
        public List<DataResponseAcert>? PrintAcert(int UsrId)
        {
            var array = CheckAcert(UsrId);
            int isAcert = (int)array.GetValue(0)!;
            int numB = (int)array.GetValue(1)!;
            if (isAcert == 1)
            {
                return new SaloonData().GetPropertiesOfAcert(UsrId, numB);
            }
            else
            {
                return null;
            }
        }

        public WinningM IsLineWin(int usrId, string? winning)
        {
            //Patterns to check with NumIds
            int[] patternL1 = { 1, 4, 7, 10, 13, 16, 19, 22, 25 };
            int[] patternL2 = { 2, 5, 8, 11, 14, 17, 20, 23, 26 };
            int[] patternL3 = { 3, 6, 9, 12, 15, 18, 21, 24, 27 };
            
            var tk = new SaloonData().GetActiveListOfTickets(usrId);
            foreach (var item in tk)
            {
                var numIds = new SaloonData().AllAcertForCurrentTicket(usrId, item.TicketID);
                int[] intersecOne = patternL1.Intersect(numIds).ToArray();
                int[] intersecTwo = patternL2.Intersect(numIds).ToArray();
                int[] intersecThree = patternL3.Intersect(numIds).ToArray();
                var fiveOne = intersecOne.Length.Equals(5);
                var fiveTwo = intersecTwo.Length.Equals(5);
                var fiveThree = intersecThree.Length.Equals(5);

                if (fiveOne && fiveTwo && fiveThree && (winning != "Bingo")) //This is Bingo.
                {
                    bool line = false;
                    bool bingo = true;
                    new SaloonData().CreateTHistory(item.TicketID,item.Identificator, usrId, item.TicketNo, line, bingo);
                    return new WinningM
                    {
                        TicketID = item.TicketID,
                        IsBingo = true,
                        IsLine = false,
                        Numbers = item.Numbers
                    };
                }
                else if ((fiveOne || fiveTwo || fiveThree)&& (winning !="Line")) //This is Line.
                {
                    bool line = true;
                    bool bingo = false;
                    new SaloonData().CreateTHistory(item.TicketID,item.Identificator, usrId, item.TicketNo, line, bingo); 
                    if (fiveOne)
                    {
                        return new WinningM
                        {
                            TicketID = item.TicketID,
                            IsBingo = false,
                            IsLine = true,
                            Numbers = item.Numbers
                        };
                    }
                    else if (fiveTwo)
                    {
                        return new WinningM
                        {
                            TicketID = item.TicketID,
                            IsBingo = false,
                            IsLine = true,
                            Numbers = item.Numbers
                        };
                    }
                    else
                    {
                        return new WinningM
                        {
                            TicketID = item.TicketID,
                            IsBingo = false,
                            IsLine = true,
                            Numbers = item.Numbers
                        };
                    }
                }
            }
            return new WinningM
            {
                TicketID = 0,
                IsBingo = false,
                IsLine = false,
                Numbers = null!
            };
        }
        public IEnumerable<TicketHistory> GetTHistory(int usrId)
        {
            var data = new SaloonData().GetTicketHistories(usrId);
            return data;
        }
    }    
}