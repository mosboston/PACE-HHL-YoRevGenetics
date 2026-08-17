using System;
using UnityEngine;

[Serializable]
public class ProteinLogicBlock
{
    public enum BlockType { ELSE, IF, NESTED }

    public BlockType blockType;
    public string pieceName;
    public string action;
    public float? angle = null;
    public Color color = Color.darkGray;
    public int markerID = -1;
    public int imageLayer = 0;
}
