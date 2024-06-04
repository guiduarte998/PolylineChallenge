using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Please enter the path to the CSV file:");
        string filePath = Console.ReadLine();

        //string filePath = "E:\\Download\\input.csv"; // File path for the CSV
        var points = ReadPointsFromCSV(filePath); // Points for the polyline

        Console.WriteLine("Enter the x coordinate of the point:");
        int xCoord = int.Parse(Console.ReadLine());
        Console.WriteLine("Enter the y coordinate of the point:");
        int yCoord = int.Parse(Console.ReadLine());

        var point = (xCoord, yCoord);

        double offset = double.MaxValue;
        double totalDistance = 0;
        double station = 0;

        for (int i = 0; i < points.Length - 1; i++)
        {
            var point1 = points[i];
            var point2 = points[i + 1];

            double segmentDistance = CalculateDistanceBetweenPoints(point1, point2);
            if (IsPointPerpendicularToSegment(point1, point2, point))
            {
                double distance = CalculatePerpendicularDistance(point1, point2, point);
                if (distance < offset)
                {
                    offset = distance;
                    station = totalDistance + CalculateDistanceBetweenPoints(point1, CalculateIntersectionPoint(point1, point2, point));
                }

                Console.WriteLine($"The point ({xCoord}, {yCoord}) is perpendicular between the points {point1} and {point2}");
            }

            totalDistance += segmentDistance;
        }

        if (offset == double.MaxValue)
        {
            Console.WriteLine("No valid perpendicular found.");
            Console.WriteLine("Finished");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine($"The smallest perpendicular distance is {offset:F2}");
            Console.WriteLine($"The station distance is {station:F2}");
            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }

    // Reads points from the CSV file
    static (int, int)[] ReadPointsFromCSV(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var points = new (int, int)[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            int x = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int y = int.Parse(parts[1], CultureInfo.InvariantCulture);
            points[i] = (x, y);
        }
        return points;
    }

    // Calculates the Euclidean distance between two points
    static double CalculateDistanceBetweenPoints((int, int) point1, (int, int) point2)
    {
        int x1 = point1.Item1, y1 = point1.Item2;
        int x2 = point2.Item1, y2 = point2.Item2;

        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    // Defines the equation of the line passing through two points
    static (double, double) DefineLine((int, int) point1, (int, int) point2)
    {
        int x1 = point1.Item1, y1 = point1.Item2;
        int x2 = point2.Item1, y2 = point2.Item2;

        // Calculate the slope
        double m = (double)(y2 - y1) / (x2 - x1);

        // Calculate the y-intercept
        double c = y1 - m * x1;

        return (m, c);
    }

    // Calculates the y-intercept of a line passing through a point with a given slope
    static double CalculateYIntercept((int, int) point, double slope)
    {
        int x = point.Item1, y = point.Item2;

        // Calculate the y-intercept
        return y - slope * x;
    }

    // Checks if a point is between two lines
    static bool IsPointBetweenLines((int, int) point, (double, double) line1, (double, double) line2)
    {
        int x0 = point.Item1, y0 = point.Item2;
        double m1 = line1.Item1, c1 = line1.Item2;
        double m2 = line2.Item1, c2 = line2.Item2;

        // Calculate the y-values for the two lines at x0
        double y1 = m1 * x0 + c1;
        double y2 = m2 * x0 + c2;

        // Check if y0 is between y1 and y2
        return Math.Min(y1, y2) <= y0 && y0 <= Math.Max(y1, y2);
    }

    // Main function to check if a point is perpendicular to a line segment
    static bool IsPointPerpendicularToSegment((int, int) point1, (int, int) point2, (int, int) point)
    {
        var (m, c) = DefineLine(point1, point2);

        double m_perpendicular = -1.0 / m;

        var line1 = (m_perpendicular, CalculateYIntercept(point1, m_perpendicular));
        var line2 = (m_perpendicular, CalculateYIntercept(point2, m_perpendicular));

        return IsPointBetweenLines(point, line1, line2);
    }

    // Calculates the perpendicular distance from a point to a line segment
    static double CalculatePerpendicularDistance((int, int) point1, (int, int) point2, (int, int) point)
    {
        int x1 = point1.Item1, y1 = point1.Item2;
        int x2 = point2.Item1, y2 = point2.Item2;
        int x0 = point.Item1, y0 = point.Item2;

        double numerator = Math.Abs((y2 - y1) * x0 - (x2 - x1) * y0 + x2 * y1 - y2 * x1);
        double denominator = Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));
        return numerator / denominator;
    }

    // Calculates the intersection point of a perpendicular from a point to a line segment
    static (int, int) CalculateIntersectionPoint((int, int) point1, (int, int) point2, (int, int) point)
    {
        var (m, c) = DefineLine(point1, point2);
        double m_perpendicular = -1.0 / m;
        double c_perpendicular = point.Item2 - m_perpendicular * point.Item1;

        double x_intersection = (c_perpendicular - c) / (m - m_perpendicular);
        double y_intersection = m * x_intersection + c;

        return ((int)x_intersection, (int)y_intersection);
    }
}
