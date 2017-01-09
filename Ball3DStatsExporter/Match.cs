using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball3DStatsExporter
{
    public class Match
    {
        public List<Team> listOfTeams { get; set; }

        public string date { get; set; }
        public string time { get; set; }
        public string csvReturnText { get; set; }
    }
}
