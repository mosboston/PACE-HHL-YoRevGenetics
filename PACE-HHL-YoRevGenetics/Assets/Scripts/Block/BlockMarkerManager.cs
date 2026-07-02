using FAST;
using UnityEngine;

using Application = FAST.Application;

public class BlockMarkerManager : MonoBehaviour
{
    [SerializeField] bool showVisuals;
    [SerializeField] Transform blockMarkerParent;
    [SerializeField] BlockMarker blockMarkerPrefab;

    private BlockMarker[] blockMarkers;

    private void Start()
    {
        MarkerTrackingSettings settings = Application.settings.markerTrackingSettings;

        blockMarkers = new BlockMarker[settings.maxNumMarkers];
        for (int i = 0; i < settings.maxNumMarkers; i++)
        {
            blockMarkers[i] = Instantiate(blockMarkerPrefab, blockMarkerParent);
            blockMarkers[i].SetMarkerID(i);
        }

        UpdateVisualStatus();
    }

    public void ToggleBlockMarkerVisuals()
    {
        showVisuals = !showVisuals;
        UpdateVisualStatus();
    }

    private void UpdateVisualStatus()
    {
        foreach (BlockMarker block in blockMarkers)
        {
            block.showVisual = showVisuals;
        }
    }
}
