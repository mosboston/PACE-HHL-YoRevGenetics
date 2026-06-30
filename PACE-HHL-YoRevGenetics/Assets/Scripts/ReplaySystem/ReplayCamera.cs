using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ReplayCamera : MonoBehaviour
{
    [SerializeField] RawImage testImage;

    [SerializeField] RectTransform captureArea;
    Rect CaptureAreaScreenRect { get
        {
            Vector3[] corners = new Vector3[4];
            captureArea.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++)
                corners[i] = cam.WorldToScreenPoint(corners[i]);

            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float width = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x) - minX;
            float height = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y) - minY;

            // Display the screen space rectangle
            return new(minX, minY, width, height);
        }
    }

    Camera cam;

    public void TestFunc()
    {
        StartCoroutine(CaptureFrame());
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public IEnumerator CaptureFrame()
    {
        yield return new WaitForEndOfFrame();

        int width = (int)CaptureAreaScreenRect.width;
        int height = (int)CaptureAreaScreenRect.height;

        Texture2D frame = new(width, height);
        frame.ReadPixels(CaptureAreaScreenRect, 0, 0);
        frame.Apply();

        testImage.texture = frame;
    }
}
