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
    [XmlIgnore]
    public Dictionary<string, ProteinAnimation> proteinAnimations;

    public ProteinLogicBlock GetLogicBlockByMarkerID(int markerID)
    {
        ProteinLogicBlock[] options = allProtienLogic.Where(kv => kv.Value.markerID == markerID).Select(kv => kv.Value).ToArray();
        return options.Length > 0 ? options[0] : null;
    }

    public ProteinLogicBlock FindLogicBlock(string blockName)
    {
        if (!allProtienLogic.TryGetValue(blockName, out ProteinLogicBlock result))
            result = null;
        return result;
    }

    public string defaultAction = "fail";
    public int startBlockID = 22;
    public int endBlockID = 23;

    [XmlIgnore]
    public Dictionary<string, RectTransform> pointsOfInterest = new();

    [XmlIgnore]
    public MarkerTrackingSettings markerTrackingSettings = new();
}
