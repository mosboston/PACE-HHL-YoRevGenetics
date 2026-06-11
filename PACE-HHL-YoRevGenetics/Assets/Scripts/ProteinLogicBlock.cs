using System;
using UnityEngine;

[Serializable]
public class ProteinLogicBlock
{
    public enum BlockType { ELSE, IF, NESTED, DEFAULT }

    public BlockType blockType;
    public string pieceName;
    public string action;
    public float? angle = null;
    public Color color = Color.darkGray;
}
