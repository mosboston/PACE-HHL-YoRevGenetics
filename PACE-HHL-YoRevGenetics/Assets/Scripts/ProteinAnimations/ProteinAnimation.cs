using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class ProteinAnimation
{
    public string name;
    [XmlElement(Type = typeof(AnimationCommand))]
    [XmlElement(Type = typeof(MoveProteinToCommand))]
    public List<AnimationCommand> animationCommands;
}
