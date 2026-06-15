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

    public string defaultAction = "fail";
    public int startBlockID;
    public int endBlockID;

    [XmlIgnore]
    public Dictionary<string, RectTransform> pointsOfInterest = new();

    [XmlIgnore]
    public MarkerTrackingSettings markerTrackingSettings = new();

    public ProteinLogicBlock FindLogicBlock(string blockName)
    {
        ProteinLogicBlock block = orderedProteinLogic.Find(b => b.pieceName.Equals(blockName));
        //if (block == null)
        //    Debug.LogError($"Could not find block with name '{blockName}' in {Application.skin}-proteinLogic.xml");
        return block;
    }
}
