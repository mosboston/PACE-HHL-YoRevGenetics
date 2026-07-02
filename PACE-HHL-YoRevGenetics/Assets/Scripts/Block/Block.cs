using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Application = FAST.Application;

public class Block : MonoBehaviour
{
    public string blockName;

    public bool IsStart { get => blockName.Equals("START"); }
    public bool IsEnd { get => blockName.Equals("END"); }

    protected virtual void Awake()
    {
        SetName(blockName);
    }

    public virtual void SetName(string name)
    {
        blockName = name;
        this.name = $"{blockName} Block";
    }
}
