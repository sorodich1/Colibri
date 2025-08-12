using Colibri.GetDirection.Helpers;
using Newtonsoft.Json;
using System;

namespace Colibri.GetDirection
{
    public class DirectionJson
    {
        public static string RouteFly(Point start, Point stop)
        {
            var distance = Math.Round(ProcessRequest.CalculateDistance(start, stop), 3);

            var distanceList = ProcessRequest.GenerateRoutePoints(start, stop, 100);

            GeodistanceRequest geodistanceRequest = new()
            {
                Start = start,
                End = stop,
                Distance = distance,
                RoutePoints = distanceList,
                Route = "По прямой"
            };

            return JsonConvert.SerializeObject(geodistanceRequest);
        }
    }
}
