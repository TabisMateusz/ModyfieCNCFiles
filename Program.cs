using System.Text.RegularExpressions;

namespace ModyfieCNCFiles
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cncFiles = Directory.GetFiles("c:\\Users\\matau\\OneDrive\\Pulpit\\projekt\\CNC\\");
            foreach (var cncFile in cncFiles)
            {
                //get all lines from cnc file
                var linesInFile = File.ReadAllLines(cncFile);

                //get only lines where are information about hard stump 
                var hardStumpMarksLinePositions = linesInFile.Select((line, index) => new { line = line, index = index })
                    .Where(x => x.line.Contains("_999"))
                    .Select(x => new { newLine = x.line.Replace("_999", ""), x.index }).ToList();

                var pointsLint = getPiontsCoordination(linesInFile);

                File.WriteAllLines(cncFile, linesInFile);

            }
        }

        static private Dictionary<char, List<Point>> getPiontsCoordination(string[] linesInFile)
        {
            List<string> linesWithContureMarkCoordinate = new List<string>();

            for (int i = 0; i < linesInFile.Length; i++)
            {
                if (linesInFile[i].Contains("KO"))
                {
                    linesWithContureMarkCoordinate.Add(linesInFile[i + 1]);
                    linesWithContureMarkCoordinate.Add(linesInFile[i + 2]);
                }
            }

            int letter = 65;
            string pattern = @"\d+\.\d+";

            Dictionary<char, List<Point>> pointList = new Dictionary<char, List<Point>>();
            foreach (var line in linesWithContureMarkCoordinate)
            {
                MatchCollection match = Regex.Matches(line, pattern);

                if (!pointList.ContainsKey((char)letter))
                    pointList.Add((char)letter, new List<Point>
                        { new Point
                        {
                            Xposition = int.Parse(match[0].Value.Substring(0, match[0].Length - 3)),
                            Yposition = int.Parse(match[1].Value.Substring(0, match[1].Length - 3))
                        } });
                letter++;
            }
            foundCrossPointTwoLine(pointList);
            return pointList;
        }

        static private void foundCrossPointTwoLine(Dictionary<char, List<Point>> pointList)
        {
            for (int i = 0; i < pointList.Count; i++) 
            {
                
            
            
            }


        }
    }
}
