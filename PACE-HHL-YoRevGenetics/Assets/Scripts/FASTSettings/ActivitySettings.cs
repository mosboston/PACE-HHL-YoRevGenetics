using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using FAST;
using UnityEngine;

using Application = FAST.Application;

public class ActivitySettings : BaseSettings
{
    public List<ProteinLogicBlock> orderedProteinLogic;
    public string defaultAction;

    [XmlIgnore]
    public Dictionary<string, Vector2> pointsOfInterest = new();

    public ProteinLogicBlock FindLogicBlock(string blockName)
    {
        ProteinLogicBlock block = orderedProteinLogic.Find(b => b.pieceName.Equals(blockName));
        //if (block == null)
        //    Debug.LogError($"Could not find block with name '{blockName}' in {Application.skin}-proteinLogic.xml");
        return block;
    }
}
