using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class ProteinAnimation
{
    public string name;
    [XmlElement(Type = typeof(MoveProteinToCommand))]
    [XmlElement(Type = typeof(OrientToAngleCommand))]
    [XmlElement(Type = typeof(FullResetCommand))]
    public List<AnimationCommand> animationCommands;
}
