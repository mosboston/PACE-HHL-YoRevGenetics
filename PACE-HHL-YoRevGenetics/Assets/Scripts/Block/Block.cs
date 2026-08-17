using UnityEngine;

public class Block : MonoBehaviour
{
    public string blockName;

    public bool IsStart { get => blockName.Equals("START"); }
    public bool IsEnd { get => blockName.Equals("END"); }
    public bool IsAll { get => blockName.Equals("DBG_ALLBLOCK"); }

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
