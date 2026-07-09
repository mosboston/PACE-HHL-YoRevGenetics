using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationCommand
{
    public abstract IEnumerator RunCommand(Dictionary<string, object> args);

    public const string kProtein = "Protein";
    public const string kAngle = "Angle";
}

public abstract class TimedCommand : AnimationCommand
{
    public float commandLength = 0.75f;
    public string timeCurve = "SmoothStep";
    public Func<float, float> TimeCurve => timeCurves[timeCurve];

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        if (commandLength <= 0)
        {
            Debug.LogError("animationLength must be > 0!");
            yield break;
        }
    }

    public static Dictionary<string, Func<float, float>> timeCurves = new()
    {
        { "Linear", x => x },
        { "SmoothStep", x => Mathf.SmoothStep(0.0f, 1.0f, x) },
        { "EaseInCubic", x => x * x * x },
        { "EaseOutCubic", x => { float u = (1 - x); return 1 - u * u * u; } }
    };
}

public class MoveProteinToCommand : TimedCommand
{
    public string target;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        base.RunCommand(args);

        Protein protein = args[kProtein] as Protein;
        if (!PointsOfInterest.TryGet(target, out RectTransform transform)) yield break;
        float angle = (float)args[kAngle];

        Vector2 currentPosition = protein.Position;
        float currentRotation = protein.Rotation;

        Vector2 targetPosition = transform.position;
        float targetRotation = transform.localEulerAngles.z + angle;

        float time = 0;

        while (time < commandLength)
        {
            float t = time / commandLength;

            // if animation curve is null, default to smooth step
            t = TimeCurve(t);

            protein.Rotation = Mathf.LerpAngle(currentRotation, targetRotation, t);
            protein.Position = Vector2.Lerp(currentPosition, targetPosition, t);

            time += Time.deltaTime;
            yield return null;
        }
    }
}
