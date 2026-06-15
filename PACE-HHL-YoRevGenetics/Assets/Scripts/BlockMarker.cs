using FAST;
using UnityEngine;

using Application = FAST.Application;

public class BlockMarker : Block
{
    public bool showVisual;

    [SerializeField] GameObject BlockVisual;

    public int markerID;

    protected MarkerTrackingSystem trackingSystem;

    public MarkerData MarkerData => trackingSystem.markerDataLUT[markerID];
    public bool IsTracked => MarkerData.trackingState == MarkerData.TrackingState.Tracked;

    protected override void Awake()
    {
        base.Awake();
        trackingSystem = GetComponentInParent<MarkerTrackingSystem>();

        ProteinLogicBlock block = Application.settings.GetLogicBlockByMarkerID(markerID);
        if (block != null)
        {
            SetName(block.pieceName);
        }
        else
        {
            Debug.LogError($"Could not find protein logic block with id {markerID}! Disabling this block marker");
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        BlockVisual.SetActive(showVisual && IsTracked);

        if (!IsTracked)
            return;

        Vector2 position = new(MarkerData.x, MarkerData.y);

        position -= new Vector2(0.5f, 0.5f);

        position.x *=  trackingSystem.Width;
        position.y *= -trackingSystem.Height;

        transform.position = position;
    }
}
