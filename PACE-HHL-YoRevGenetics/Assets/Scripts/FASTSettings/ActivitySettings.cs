using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using FAST;
using UnityEngine;

using Application = FAST.Application;

public class ActivitySettings : BaseSettings
{
    [XmlIgnore]
    public List<ProteinLogicBlock> orderedProteinLogic;
    [XmlIgnore]
    public Dictionary<string, ProteinLogicBlock> allProtienLogic;
    [XmlIgnore]
    public List<string> ProteinPieceNames { get => allProtienLogic.Keys.ToList(); }

    public ProteinLogicBlock GetLogicBlockByMarkerID(int markerID)
    {
        return allProtienLogic.First(kv => kv.Value.markerID == markerID).Value;
    }

    public string defaultAction = "fail";
    public int startBlockID = 22;
    public int endBlockID = 23;

    [XmlIgnore]
    public Dictionary<string, RectTransform> pointsOfInterest = new();

    [XmlIgnore]
    public MarkerTrackingSettings markerTrackingSettings = new();
}
