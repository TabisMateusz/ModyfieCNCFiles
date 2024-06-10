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
                    .Where(x => x.line.Contains("_999")).ToArray();

                List<Point> hardStumpPoints = new List<Point>();
                foreach(var element in hardStumpMarksLinePositions)
                {

                    modyfieHardStumpPosition(
                        getHardStumpPointPossition(element.line), element.line);
                }



                var crossPoint = getCrossPoint(linesInFile);
                
                File.WriteAllLines(cncFile, linesInFile);
            }
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
            int y = (line1[0] * line2[2] - line2[0] * line2[0]) / det ;

            if(x<0) x *= -1 ;
            if(y<0) x *= -1 ;

            //inster x and y value and return
            return new Point {Xposition = x,Yposition = y };

            
        }

        static private Point getHardStumpPointPossition(string hardStampInfo)
        {
            string pattern = @"\d+\.\d+";
            MatchCollection match = Regex.Matches(hardStampInfo, pattern);

            return new Point
            {
                Xposition = int.Parse(match[0].Value.Substring(0, match[0].Length - 3)),
                Yposition = int.Parse(match[1].Value.Substring(0, match[1].Length - 3))
            };
        }

        static private void modyfieHardStumpPosition(Point hardStumpPosistion, string hardStumpInfo)
        {
            var splitedLine = hardStumpInfo.Split(' ');

            
        }
    }
}
