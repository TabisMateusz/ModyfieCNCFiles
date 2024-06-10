using System.Text.RegularExpressions;

namespace ModyfieCNCFiles
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cncFiles = Directory.GetFiles("d:\\AUTOZAPIS\\CNC\\");
            int completed = 0;
            int notCompleted = 0;
            foreach (var cncFile in cncFiles)
            {
                //get all lines from cnc file
                var linesInFile = File.ReadAllLines(cncFile);

                //get only lines where are information about hard stump 
                var hardStumpMarksLinePositions = linesInFile.Select((line, index) => new { line = line, index = index })
                    .Where(x => x.line.Contains("_999")).ToArray();

                if (hardStumpMarksLinePositions.Length == 1)
                {
                    try
                    {
                        linesInFile[hardStumpMarksLinePositions[0].index] = hardStumpMarksLinePositions[0].line.Replace("_999", "");
                        completed++;
                    }
                    catch
                    {
                        Console.WriteLine($"For {Path.GetFileName(cncFile)} can not modyfie hardStump");
                        notCompleted++;
                    }
                }
                if (hardStumpMarksLinePositions.Length == 2)
                {
                    try
                    {
                        foreach (var element in hardStumpMarksLinePositions)
                        {
                            linesInFile[element.index] = modyfieHardStumpPosition(getCrossPoint(linesInFile), element.line);
                            completed++;

                        }
                    }
                    catch
                    {
                        Console.WriteLine($"For {Path.GetFileName(cncFile)} can not modyfie hardStump");
                        notCompleted++;

                    }
                }
                if (hardStumpMarksLinePositions.Length == 3)
                {

                }

                File.WriteAllLines(cncFile, linesInFile);
            }

            Console.WriteLine($"Modifications completed. Completed CNC {completed}, not compleated CNC {notCompleted}. If any not compleated, please check it manualy.");
        }

        static private Point getCrossPoint(string[] linesInFile)
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
           
            string pattern = @"\d+\.\d+";

            List<Point> pointList = new List<Point>();
            foreach (var line in linesWithContureMarkCoordinate)
            {
                MatchCollection match = Regex.Matches(line, pattern);


                pointList.Add(new Point
                {
                    Xposition = int.Parse(match[0].Value.Substring(0, match[0].Length - 3)),
                    Yposition = int.Parse(match[1].Value.Substring(0, match[1].Length - 3))
                });
                
            }
           return foundCrossPointTwoLine(pointList);
        }

        static private List<int> findLine(Point point1, Point point2) 
        {
            //Ax + By = C
            //A=y2-y1
            //B=x1-x2
            //C=x2*y1 - x1*y2

            List<int> pointList = new List<int>();
            pointList.Add(point2.Yposition - point1.Yposition);
            pointList.Add(point2.Xposition - point1.Xposition);
            pointList.Add(point2.Xposition * point1.Yposition - point1.Xposition * point2.Yposition);

            return pointList;
        }

        static private Point foundCrossPointTwoLine(List<Point> pointList)
        {
            //Ax + By = C
            //A=y2-y1
            //B=x1-x2
            //C=x2*y1 - x1*y2
            //pointList[0] = x1 pointList[1] = y1 pointList[2] =x2 pointList[3]=y2
            //pointList[0]=A
            //pointList[1]=B
            //pointList[2]=C
            //pointList[3]=D

            //line[0] - a, line[1] - b, line[2] - c
            var line1 = findLine(pointList[0], pointList[1]);
            var line2 = findLine(pointList[2], pointList[3]);

            int det = line1[0] * line2[1] - line2[0] * line1[1];

            //count cross point from formula
            int x = (line2[1] * line1[2] - line1[1] * line2[2]) / det ;
            int y = (line1[0] * line2[2] - line2[0] * line1[2]) / det ;

            if(x<0) x *= -1 ;
            if(y<0) x *= -1 ;

            //inster x and y value and return
            return new Point {Xposition = x,Yposition = y };

            
        }
        static private string modyfieHardStumpPosition(Point crossPoint, string hardStumpInfo)
        {
           var newCrossPoint = getHardStumpRotation(hardStumpInfo,crossPoint);

            string pattern = @"\b\d+\.\d+s?\b";

            string[] newValues = { ((newCrossPoint.Xposition)+".00s").ToString(), ((newCrossPoint.Yposition)+".00").ToString() };

            // Variable to keep track of replacement index
            int replacementIndex = 0;

            // MatchEvaluator to replace matched values with new values
            MatchEvaluator evaluator = match =>
            {
                if (replacementIndex < newValues.Length)
                {
                    return newValues[replacementIndex++];
                }
                return match.Value; // return original value if no more replacements
            };

            return Regex.Replace(hardStumpInfo, pattern, evaluator).Replace("_999","");
        }
        static private Point getHardStumpRotation(string hardStumpInfo,Point crossPoint)
        {
            string patternMimus = @"-\d+\.\d+";
            MatchCollection match = Regex.Matches(hardStumpInfo, patternMimus);
            string rotaion = " ";

            if (match.Count == 1)
                rotaion = match[0].Value;
            else
            {
                string pattern = @"\d+\.\d+";
                MatchCollection match2 = Regex.Matches(hardStumpInfo, pattern);
                rotaion = match2[2].Value;

            }

            if (rotaion == "90.00")
                return new Point { Xposition = crossPoint.Xposition -10, Yposition = crossPoint.Yposition + 10};
            if (rotaion == "-90.00")
                return new Point { Xposition = crossPoint.Xposition + 10, Yposition = crossPoint.Yposition - 10};
            if (rotaion == "180.00")
                return new Point { Xposition = crossPoint.Xposition - 10, Yposition = crossPoint.Yposition -10 };
            if (rotaion == "-180.00")
                return new Point { Xposition = crossPoint.Xposition - 10, Yposition = crossPoint.Yposition - 10};
            return new Point();
        }
    }
}
