using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ReplayData
{
    public int replayWidth;
    public int replayHeight;
    public float timePerFrame;
    public List<Texture2D> frames;
    public bool doneRecording;
}

[RequireComponent(typeof(Camera))]
public class ReplayCamera : MonoBehaviour
{
    public Action<ReplayData> onRecordingDone;

    [Header("Settings")]
    [Tooltip("frames/sec")]
    float framerate = 24;

    [Header("References")]
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
    bool isRecording;
    ReplayData latestReplay;

    public void StartRecording()
    {
        isRecording = true;

        latestReplay = new()
        {
            replayWidth = (int)CaptureAreaScreenRect.width,
            replayHeight = (int)CaptureAreaScreenRect.height,
            timePerFrame = 1/framerate,
            frames = new(),
            doneRecording = false,
        };

        StartCoroutine(Record());
    }

    public void StopRecording()
    {
        latestReplay.doneRecording = true;
        isRecording = false;

        onRecordingDone?.Invoke(latestReplay);
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private IEnumerator Record()
    {
        while (isRecording)
        {
            StartCoroutine(CaptureFrame(latestReplay));
            yield return new WaitForSeconds(latestReplay.timePerFrame);
        }
    }

    public IEnumerator CaptureFrame(ReplayData replayData)
    {
        yield return new WaitForEndOfFrame();

        int width = (int)CaptureAreaScreenRect.width;
        int height = (int)CaptureAreaScreenRect.height;

        Texture2D frame = new(width, height);
        frame.ReadPixels(CaptureAreaScreenRect, 0, 0);
        frame.Apply();

        replayData.frames.Add(frame);
    }
}
