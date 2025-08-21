using System.Collections.Generic;

namespace Colibri.GetDirection.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class GeoPoint
    {
        public double Latitude {  get; set; }
        public double Longitude {  get; set; }
    }

    //public class Poligon
    //{
    //    public bool Inclusion { get; set; }
    //    public List<List<GeoPoint>> Polygons {  get; set; }
    //    public int Version {  get; set; }
    //}

    public class MissionItem
    {
        public object AMSLAltAboveTerrain {  get; set; }
        public double Altitude {  get; set; }
        public int AltitudeMode {  get; set; }
        public bool AutoContine {  get; set; }
        public int Command { get; set; }
        public int DoJumpId { get; set; }
        public int Frame { get; set; }
        public List<object> Params {  get; set; }
        public string Type {  get; set; }
    }

    public class Mission
    {
        public double CruiseSpeed { get; set; }
        public int FirmwareType { get; set; }
        public int GlobalPlanAltitudeMode { get; set; }
        public double HoverSpeed { get; set; }
        public List<MissionItem> Items { get; set; }
        public List<double> PlannedHomePosition { get; set; }
        public int VehicleType { get; set; }
        public int Version { get; set; }
    }

    public class PlanModel
    {
        public string FileType { get; set; }
        public string GroundStation {  get; set; }
        public Mission Mission {  get; set; }
        public int Version { get; set; }
    }
}
