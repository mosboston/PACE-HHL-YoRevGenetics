using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Application = FAST.Application;

public class Block : MonoBehaviour
{
    public string blockName;

    public bool IsStart { get => blockName.Equals("START"); }
    public bool IsEnd { get => blockName.Equals("END"); }

    private Image image;

    private void Awake()
    {
        SetName(blockName);

        image = GetComponent<Image>();
    }

    public void SetName(string name)
    {
        blockName = name;
        this.name = $"{blockName} Block";

        if (Application.settings.allProtienLogic.TryGetValue(blockName, out ProteinLogicBlock logicBlock))
        {
            Color color = logicBlock.color;
            //color.a = 1;
            image.color = color;
        }
    }

    // TEMP BLOCK MOVING CODE
    Canvas parentCanvas;
    RectTransform parentCanvasRectTransform;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        parentCanvasRectTransform = parentCanvas.GetComponent<RectTransform>();
    }

    private void OnMouseDrag()
    {
        Vector3 viewPortMousePos = new(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
        viewPortMousePos -= new Vector3(0.5f, 0.5f); // center viewport pos
        Vector2 scale = parentCanvasRectTransform.sizeDelta;
        transform.position = Vector3.Scale(viewPortMousePos, scale);
    }
}
