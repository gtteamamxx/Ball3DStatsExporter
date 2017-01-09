using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ball3DStatsExporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Match Match;
       
        public MainWindow()
        {
            InitializeComponent();

            Match = new Match();

            textbox_BlueTeamCode.TextChanged += (s, e) =>
            {
                button_Convert.IsEnabled = textbox_BlueTeamCode.Text.Length > 0;
            };

            textbox_RedTeamCode.TextChanged += (s, e) =>
            {
                button_Convert.IsEnabled = textbox_BlueTeamCode.Text.Length > 0;
            };

            textbox_SourceCode.TextChanged += (s, e) =>
            {
                button_Convert.IsEnabled = textbox_BlueTeamCode.Text.Length > 0;
            };
        }

        private void button_Convert_Click(object sender, RoutedEventArgs e)
        {
            GetDataAndFormat();
            ShowSaveFileDialog();
        }

        private void GetDataAndFormat()
        {
            var codeText = textbox_SourceCode.Text;

            string status = codeText.Substring(codeText.IndexOf("GAME ENDED"), 29).Remove(0,13).Trim();

            var date = status.Substring(0, 10).Trim();
            Match.time = status.Replace(date, "").Trim();
            var newdate = date.Split('.');
            Match.date = $"{newdate[2]}.{newdate[1]}.{newdate[0]}";
            

            Match.listOfTeams = new List<Team>();

            var teamRed = new Team() { name = textbox_RedTeamCode.Text };
            var teamBlue = new Team() { name = textbox_BlueTeamCode.Text };

            var playerList = GetListOfPlayers(codeText);

            teamRed.listOfPlayers = playerList[0];
            teamBlue.listOfPlayers = playerList[1];

            Match.listOfTeams.Add(teamRed);
            Match.listOfTeams.Add(teamBlue);

            GetGoalsInfo(Match.listOfTeams, codeText);

            StringBuilder stringBuilder = new StringBuilder();

        }

        private void GetGoalsInfo(List<Team> teams, string codeText)
        {
            string points = codeText.Substring(codeText.IndexOf("RED"), codeText.IndexOf("--------------------------------------------") - 1);
            string[] splittedPoints = Regex.Replace(points, @"\s+", " ").Split(' ');
            teams[0].pointsFirstHalf = teams[0].pointsSumOfHalfs = splittedPoints[1];
            teams[1].pointsFirstHalf = teams[1].pointsSumOfHalfs = splittedPoints[3];
        }

        private Player GetDetailInfoAboutPlayer(Player player, string codeText)
        {
            int playerIndex = codeText.IndexOf($"{player.name}:");
            var lenght = codeText.IndexOf("--------------------------------------------", playerIndex);

            string stringOfPlayer = codeText.Substring(playerIndex,  (lenght == -1 ? codeText.Length : lenght) - playerIndex).Remove(0, player.name.Length+1).Replace("\r\n", " ");

            string[] splittedPlayer = Regex.Replace(stringOfPlayer, @"\s+", " ").Split(' ');

            for(int i = 0; i < splittedPlayer.Length; i++)
            {
                if(splittedPlayer[i] == "Assists:")
                {
                    player.assists = int.Parse(splittedPlayer[i + 1]);
                }
                else if(splittedPlayer[i] == "Goals:")
                {
                    player.goals = int.Parse(splittedPlayer[i + 1]);
                }
                else if(splittedPlayer[i] == "Kick" &&
                    splittedPlayer[i+1] == "Accuracy:")
                {
                    player.kickAccuracy = splittedPlayer[i + 2].Replace("%", "");
                }
                else if (splittedPlayer[i] == "Ranking" &&
                    splittedPlayer[i + 1] == "Points:")
                {
                    player.points = int.Parse(splittedPlayer[i + 2]);
                }
            }

            return player;
        }
        private void ShowSaveFileDialog()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = $"{Match.listOfTeams[0].name}_vs_{Match.listOfTeams[1].name}_{Match.date.Replace(".", "_")}_{Match.time}";
            dlg.DefaultExt = ".csv"; 
            dlg.Filter = "CSV file (.csv)|*.csv"; 

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                GetDataAndFormat();

                if (!filename.Contains(".csv"))
                {
                    filename += ".csv";
                }

                using (FileStream fs = File.Create(filename, 1024))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Date,Time,Venue,Teams,Results,Outcome,Players,KickAccuracy,Points,Goals,Assists\r\n");
                    sb.Append($"{Match.date.Replace(".", "/")},{Match.time}:00,,{Match.listOfTeams[0].name},{Match.listOfTeams[0].pointsFirstHalf}|{Match.listOfTeams[0].pointsSecondHalf}|{Match.listOfTeams[0].pointsSumOfHalfs},");
                    sb.Append($"{Match.listOfTeams[0].GetMatchText(Match.listOfTeams[1])},{Match.listOfTeams[0].listOfPlayers[0].name},");
                    sb.Append($"{Match.listOfTeams[0].listOfPlayers[0].kickAccuracy},{Match.listOfTeams[0].listOfPlayers[0].points.ToString()},{Match.listOfTeams[0].listOfPlayers[0].goals.ToString()},");
                    sb.Append($"{Match.listOfTeams[0].listOfPlayers[0].assists.ToString()}");

                    for(int i = 1; i < Match.listOfTeams[0].listOfPlayers.Count; i++)
                    {
                        var player = Match.listOfTeams[0].listOfPlayers[i];
                        sb.Append($"\r\n,,,,,,{player.name},{player.kickAccuracy},{player.points.ToString()},{player.goals.ToString()},{player.assists.ToString()}");
                    }

                    sb.Append($"\r\n,,,{Match.listOfTeams[1].name},{ Match.listOfTeams[1].pointsFirstHalf}|{ Match.listOfTeams[1].pointsSecondHalf}|{ Match.listOfTeams[1].pointsSumOfHalfs},");
                    sb.Append($"{Match.listOfTeams[1].GetMatchText(Match.listOfTeams[0])},{Match.listOfTeams[1].listOfPlayers[0].name},");
                    sb.Append($"{Match.listOfTeams[1].listOfPlayers[0].kickAccuracy},{Match.listOfTeams[1].listOfPlayers[0].points.ToString()},{Match.listOfTeams[1].listOfPlayers[0].goals.ToString()},");
                    sb.Append($"{Match.listOfTeams[1].listOfPlayers[0].assists.ToString()}");

                    for (int i = 1; i < Match.listOfTeams[1].listOfPlayers.Count; i++)
                    {
                        var player = Match.listOfTeams[1].listOfPlayers[i];
                        sb.Append($"\r\n,,,,,,{player.name},{player.kickAccuracy},{player.points.ToString()},{player.goals.ToString()},{player.assists.ToString()}");
                    }

                    Byte[] text = new UTF8Encoding(true).GetBytes(sb.ToString());
                    fs.Write(text, 0, text.Length);
                }
            }
        }

        private List<Player>[] GetListOfPlayers(string codeText)
        {
            List<Player>[] listOfPlayers = new List<Player>[2];
            listOfPlayers[0] = new List<Player>();
            listOfPlayers[1] = new List<Player>();

            for (int n = 0; n < 2; n++)
            {
                string stringOfPlayers = string.Empty;

                if (n == 0)
                {
                    int playerRedPlayersIndex = codeText.IndexOf("RED Players:");
                    stringOfPlayers = codeText.Substring(playerRedPlayersIndex, codeText.IndexOf("--------------------------------------------", playerRedPlayersIndex) - playerRedPlayersIndex).Remove(0, 12).Replace("\r\n", " ");
                }
                else
                {
                    int playerBluePlayersIndex = codeText.IndexOf("BLUE Players:");
                    stringOfPlayers = codeText.Substring(playerBluePlayersIndex, codeText.IndexOf("--------------------------------------------", playerBluePlayersIndex) - playerBluePlayersIndex).Remove(0, 12).Replace("\r\n", " ");
                }

                string[] splittedPlayerss = Regex.Replace(stringOfPlayers, @"\s+", " ").Split(' ');

                Player tempPlayer = new Player();
                for (int i = 1; i < splittedPlayerss.Length; i++)
                {
                    if ((i - 1) % 5 == 0) // country
                    {
                        tempPlayer = new Player();
                    }
                    else if((i - 1) % 5 == 3) // login
                    {
                        tempPlayer.name = splittedPlayerss[i];

                        GetDetailInfoAboutPlayer(tempPlayer, codeText);
                        listOfPlayers[n].Add(tempPlayer);
                    }
                }
            }
            return listOfPlayers;
        }
    }
}
