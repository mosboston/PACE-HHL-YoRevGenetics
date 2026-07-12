using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class ProteinAnimation
{
    public string name;
    [XmlElement(Type = typeof(MoveProteinToTargetCommand))]
    [XmlElement(Type = typeof(MoveProteinToRandomTransformCommand))]
    [XmlElement(Type = typeof(OrientToAngleCommand))]
    [XmlElement(Type = typeof(BreakWhenInRangeCommand))]
    [XmlElement(Type = typeof(FullResetCommand))]
    public List<AnimationCommand> animationCommands;

    public void Init()
    {
        animationCommands.ForEach(c => c.Init());
    }
}
