using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class ProteinAnimation
{
    public string name;
    public string resultText = null;
    public List<AnimationCommand> animationCommands;

    public void Init()
    {
        animationCommands.ForEach(c => c.Init());
    }

    public IEnumerator PlayAnimation(Dictionary<string, object> args)
    {
        foreach (AnimationCommand command in animationCommands)
        {
            yield return command.RunCommand(args);
        }
    }
}
