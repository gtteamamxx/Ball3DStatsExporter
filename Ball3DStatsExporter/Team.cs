using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball3DStatsExporter
{
    public class Team
    {
        public enum Match_Result
        {
            Win,
            Lose,
            Draw
        }

        public string GetMatchText(Team oppositeTeam)
        {
            Match_Result status = Match_Result.Draw;

            int pointsOfATeam = int.Parse(pointsSumOfHalfs);
            int pointsOfBTeam = int.Parse(oppositeTeam.pointsSumOfHalfs);
            if (pointsOfATeam > pointsOfBTeam)
            {
                status = Match_Result.Win;
            }
            else if(pointsOfATeam < pointsOfBTeam)
            {
                status = Match_Result.Lose;
            }

            switch(status)
            {
                case Match_Result.Win: return "Win";
                case Match_Result.Lose: return "Lose";
                case Match_Result.Draw: return "Draw";
                default: return "error";
            }
        }

        public string name { get; set; }
        public string pointsFirstHalf { get; set; }
        public string pointsSecondHalf = "0";
        public string pointsSumOfHalfs { get; set; }
        public Match_Result matchResult { get; set; }

        public List<Player> listOfPlayers { get; set; }
    }
}
