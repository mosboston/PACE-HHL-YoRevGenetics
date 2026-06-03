using System;

[Serializable]
public class ProteinLogicBlock
{
    public enum BlockType { IF, NESTED, ELSE }

    public BlockType blockType;
    public string pieceName;
    public string action;
}
