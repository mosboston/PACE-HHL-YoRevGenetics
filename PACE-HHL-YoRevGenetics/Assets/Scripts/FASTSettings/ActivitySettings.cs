using System.Collections.Generic;
using System.Linq;
using FAST;


public class ActivitySettings : BaseSettings
{
    public List<ProteinLogicBlock> proteinLogic;

    public ProteinLogicBlock FindLogicBlock(string blockName)
    {
        return proteinLogic.Find(b => b.pieceName.Equals(blockName));
    }
}
