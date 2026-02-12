using Colibri.GetDirection.Helpers;
using System.Threading.Tasks;
using System;
using Colibri.GetDirection.Data;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Colibri.GetDirection
{
    /// <summary>
    /// 
    /// </summary>
    public class DirectionJson
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public static Task<GeodistanceRequest> RouteFly(Point start, Point stop)
        {
            var distance = Math.Round(ProcessRequest.CalculateDistance(start, stop), 3);

            var distanceList = ProcessRequest.GenerateRoutePoints(start, stop, 10000);

            GeodistanceRequest geodistanceRequest = new()
            {
                Start = start,
                End = stop,
                Distance = distance,
                RoutePoints = distanceList,
                Route = "По прямой"
            };

            return Task.FromResult(geodistanceRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Task<string> MissionFile(GeodistanceRequest model)
        {
            List<MissionItem> missions = [];

            int index = 0;

            foreach (var item in model.RoutePoints)
            {
                index++;

                int command = 16;

                if (index == 1)
                    command = 22;
                else if (index == model.RoutePoints.Count)
                    command = 21;

                MissionItem missionItem = new()
                {
                    Altitude = item.Latitude,
                    Params =
                    [
                        0,
                        0,
                        0,
                        null,
                        item.Latitude,
                        item.Longitude,
                        2
                    ],
                    AltitudeMode = 1,
                    AMSLAltAboveTerrain = null,
                    AutoContine = true,
                    Command = command,
                    DoJumpId = index,
                    Frame = 3,
                    Type = "SimpleItem"
                };

                missions.Add(missionItem);
            }

            Mission mission = new()
            {
                CruiseSpeed = 15,
                FirmwareType = 3,
                GlobalPlanAltitudeMode = 1,
                HoverSpeed = 5,
                Items = missions,
                PlannedHomePosition = [],
                VehicleType = 2,
                Version = 2
            };

            PlanModel planModel = new()
            {
                FileType = "MissionPlan",
                GroundStation = "QGroundControl",
                Version = 1,
                Mission = mission
            };

            string str = JsonConvert.SerializeObject(planModel, Formatting.Indented);

            return Task.FromResult(str);
        }
    }
}
