using System;

[Serializable]
public class ProteinLogicBlock
{
    public enum BlockType { ELSE, IF, NESTED, DEFAULT }

    public BlockType blockType;
    public string pieceName;
    public string action;
    public float angle = 0;
}
