using UnityEngine;
using FAST;

using Application = FAST.Application;

[RequireComponent(typeof(ImageFromFile))]
public class ProteinPiece : MonoBehaviour
{
    [SerializeField] string imageFileExtension = ".png";

    public ProteinLogicBlock LogicBlock { get; private set; }
    ImageFromFile image;

    public string BlockName { get => LogicBlock.pieceName; }

    private void Awake()
    {
        image = GetComponent<ImageFromFile>();
    }

    public void SetLogicBlock(string logicBlockName)
    {
        LogicBlock = Application.settings.FindLogicBlock(logicBlockName);

        image.baseFileName = logicBlockName + imageFileExtension;
        image.Load(Application.language);

        name = logicBlockName;
    }
}
