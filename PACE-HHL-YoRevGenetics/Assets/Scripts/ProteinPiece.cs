using UnityEngine;
using FAST;

using Application = FAST.Application;

[RequireComponent(typeof(ImageFromFile))]
public class ProteinPiece : MonoBehaviour
{
    [SerializeField] string imageFileExtension = ".png";

    ProteinLogicBlock logicBlock;
    ImageFromFile image;

    private void Awake()
    {
        image = GetComponent<ImageFromFile>();
    }

    public void SetLogicBlock(string logicBlockName)
    {
        logicBlock = Application.settings.FindLogicBlock(logicBlockName);
        image.baseFileName = logicBlockName + imageFileExtension;
        image.Load(Application.language);
    }
}
