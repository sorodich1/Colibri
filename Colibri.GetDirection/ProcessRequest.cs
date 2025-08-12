using Colibri.GetDirection.Helpers;
using System;
using System.Collections.Generic;

namespace Colibri.GetDirection
{
    public class ProcessRequest
    {
        public static List<Point> GenerateRoutePoints(Point start, Point stop, int interval)
        {
            double distance = Math.Round(CalculateDistance(start, stop) * 1000);

            int numberOfPoints = (int)(distance / interval);

            List<Point> points = [];

            for(int i = 0; i <= numberOfPoints; i++)
            {
                double fraction = (double)i / numberOfPoints;

                double lat = start.Latitude + (stop.Latitude - start.Latitude) * fraction;

                double lon = stop.Longitude + (start.Longitude - stop.Longitude) * fraction;

                points.Add(new Point
                {
                    Latitude = lat,
                    Longitude = lon,
                });
            }

            return points;
        }

        public static double CalculateDistance(Point start, Point stop)
        {
            const double R = 6371;

            double lat1 = ToRadiance(start.Latitude);
            double lon1 = ToRadiance(start.Longitude);
            double lat2 = ToRadiance(stop.Latitude);
            double lon2 = ToRadiance(stop.Longitude);

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + 
                Math.Cos(lat1) * Math.Cos(lat2) * 
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = R * c;

            return distance;
        }

        private static double ToRadiance(double radiance)
        {
            return radiance * (Math.PI / 180);
        }
    }
}
