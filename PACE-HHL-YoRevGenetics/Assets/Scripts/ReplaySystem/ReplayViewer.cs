using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ReplayViewer : MonoBehaviour
{
    [SerializeField] ReplayCamera replayCamera;

    private bool isPlaying;
    private float currentTime;
    private int currentFrame;
    private float lastFrameTime;

    RawImage image;
    ReplayData replayData;

    private void Awake()
    {
        image = GetComponent<RawImage>();
    }

    private void OnEnable()
    {
        replayCamera.onRecordingDone += PlayReplay;
    }

    private void OnDisable()
    {
        replayCamera.onRecordingDone -= PlayReplay;
    }

    public void PlayReplay(ReplayData replayData)
    {
        this.replayData = replayData;
        currentTime = 0;
        currentFrame = 0;
        lastFrameTime = 0;
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
        currentTime = 0;
        currentFrame = 0;
        lastFrameTime = 0;
    }

    public void Restart()
    {
        currentTime = 0;
        currentFrame = 0;
        lastFrameTime = 0;
    }

    private void Update()
    {
        image.enabled = isPlaying;

        if (!isPlaying)
            return;

        if (currentTime > lastFrameTime)
        {
            currentFrame++;
            lastFrameTime = currentFrame * replayData.timePerFrame;
        }

        if (currentFrame >= replayData.frames.Count)
        {
            Restart();
            return;
        }

        image.texture = replayData.frames[currentFrame];

        currentTime += Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (replayData == null || !image.enabled)
            return;

        float replayAspect = (float)replayData.width / replayData.height;
        image.rectTransform.sizeDelta = new(replayAspect * image.rectTransform.sizeDelta.y, image.rectTransform.sizeDelta.y);
    }
}
