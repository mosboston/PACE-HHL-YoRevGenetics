using FAST;
using UnityEngine;
using UnityEngine.UI;

using Application = FAST.Application;

public class BlockMarker : Block
{
    public bool showVisual;

    [SerializeField] GameObject BlockVisual;
    [SerializeField] Image image;

    public int MarkerID { get; private set; }

    protected MarkerTrackingSystem trackingSystem;

    public MarkerData MarkerData => trackingSystem.markerDataLUT[MarkerID];
    public bool IsTracked => MarkerData.trackingState != MarkerData.TrackingState.NotTracked;

    protected override void Awake()
    {
        base.Awake();
        trackingSystem = GetComponentInParent<MarkerTrackingSystem>();

        MarkerID = -1;
    }

    private void Update()
    {
        if (MarkerID < 0)
        {
            gameObject.SetActive(false);
            return;
        }

        BlockVisual.SetActive(showVisual && IsTracked);

        if (!IsTracked)
            return;

        Vector2 position = new(MarkerData.x, MarkerData.y);

        position -= new Vector2(0.5f, 0.5f);

        position.x *=  trackingSystem.Width;
        position.y *= -trackingSystem.Height;

        transform.position = position;
    }

    public void SetMarkerID(int markerID)
    {
        gameObject.SetActive(true);

        MarkerID = markerID;
        ProteinLogicBlock block = Application.settings.GetLogicBlockByMarkerID(MarkerID);
        if (block != null)
        {
            SetName(block.pieceName);
            image.color = block.color;
        }
        else if (MarkerID == Application.settings.startBlockID)
        {
            SetName("START");
            image.color = Color.green;
        }
        else if (MarkerID == Application.settings.endBlockID)
        {
            SetName("END");
            image.color = Color.red;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
