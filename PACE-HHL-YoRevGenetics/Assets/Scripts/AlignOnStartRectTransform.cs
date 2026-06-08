using FAST;
using UnityEngine;

public class AlignOnStartRectTransform : AlignmentRectTransform
{
    private Vector3 cachedOffsetPosition;
    private float cachedOffsetRotation;
    private float cachedOffsetScale;
    private Vector2 cachedOffsetSize;

    private RectTransform _rectTransform;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = transform as RectTransform;
    }

    protected virtual void Start()
    {
        cachedOffsetPosition = offsetPosition;
        cachedOffsetRotation = offsetRotation;
        cachedOffsetScale = offsetScale;
        cachedOffsetSize = offsetSize;

        base.Update();
    }

    protected override void Update()
    {
        // don't do base.Update()

        // Only move things if the offset position just changed

        if (cachedOffsetPosition != offsetPosition)
        {
            var delta = cachedOffsetPosition - offsetPosition;
            _rectTransform.anchoredPosition += (Vector2)delta;
            cachedOffsetPosition = offsetPosition;
        }

        if (cachedOffsetRotation != offsetRotation)
        {
            var delta = cachedOffsetRotation - offsetRotation;
            Quaternion rotationQuaternion = Quaternion.AngleAxis(delta, Vector3.forward);
            _rectTransform.rotation *= rotationQuaternion;
            cachedOffsetRotation = offsetRotation;
        }

        if (cachedOffsetScale != offsetScale)
        {
            var delta = cachedOffsetScale - offsetScale;
            _rectTransform.localScale *= (1f + delta);
            cachedOffsetScale = offsetScale;
        }

        if (cachedOffsetSize != offsetSize)
        {
            var delta = cachedOffsetSize - offsetSize;
            _rectTransform.sizeDelta += delta;
            cachedOffsetSize = offsetSize;
        }
    }
}
