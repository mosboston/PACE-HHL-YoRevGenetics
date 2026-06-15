using FAST;
using UnityEngine;

public class BlockMarker : Block
{
    [SerializeField] GameObject BlockVisual;

    public int markerID;

    protected MarkerTrackingSystem trackingSystem;

    public MarkerData MarkerData => trackingSystem.markerDataLUT[markerID];
    public bool IsTracked => MarkerData.trackingState == MarkerData.TrackingState.Tracked;

    protected override void Awake()
    {
        base.Awake();
        trackingSystem = GetComponentInParent<MarkerTrackingSystem>();
    }

    private void Update()
    {
        if (!IsTracked)
            return;

        Vector2 position = new(MarkerData.x, MarkerData.y);

        position -= new Vector2(0.5f, 0.5f);

        position.x *=  trackingSystem.Width;
        position.y *= -trackingSystem.Height;

        transform.position = position;
    }
}
