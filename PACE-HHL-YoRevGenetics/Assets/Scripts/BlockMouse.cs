using UnityEngine;
using UnityEngine.UI;

using Application = FAST.Application;

public class BlockMouse : Block
{

    private Image image;

    protected override void Awake()
    {
        base.Awake();

        image = GetComponent<Image>();
    }

    public override void SetName(string name)
    {
        base.SetName(name);

        if (Application.settings.allProtienLogic.TryGetValue(blockName, out ProteinLogicBlock logicBlock))
        {
            Color color = logicBlock.color;
            //color.a = 1;
            image.color = color;
        }
    }

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
