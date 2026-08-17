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

    public float Rotation
    {
        get => transform.localEulerAngles.z;
        set => transform.localEulerAngles = new(transform.localEulerAngles.x, transform.localEulerAngles.y, value);
    }

    private void Awake()
    {
        image = GetComponent<ImageFromFile>();
    }

    public void SetLogicBlock(string logicBlockName)
    {
        LogicBlock = Application.settings.FindLogicBlock(logicBlockName);

        if (LogicBlock == null)
        {
            Debug.LogError($"Logic block named {logicBlockName} could not be found!");
            return;
        }

        Rotation = LogicBlock.angle.GetValueOrDefault();

        image.baseFileName = logicBlockName + imageFileExtension;
        image.Load(Application.language);

        name = logicBlockName;
    }
}
