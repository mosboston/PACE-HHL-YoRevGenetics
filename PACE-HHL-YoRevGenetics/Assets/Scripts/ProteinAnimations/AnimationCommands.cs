using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

using Random = UnityEngine.Random;

public abstract class AnimationCommand
{
    public virtual void Init() { }

    public abstract IEnumerator RunCommand(Dictionary<string, object> args);

    public const string kProtein = "Protein";
    public const string kAngle = "Angle";

    public static Type[] kCommandTypes =
    {
        typeof(MoveProteinToTargetCommand),
        typeof(MoveProteinToRandomTransformCommand),
        typeof(MoveProteinToTargetButBreakInRangeCommand),
        typeof(OrientToAngleCommand),
        typeof(BreakWhenInRangeCommand),
        typeof(FullResetCommand),
    };
}

// ---================---
// --- Timed Commands ---
// ---================---

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

public abstract class MoveProteinToRectTransformCommand : TimedCommand
{
    protected RectTransform transform;
    protected Func<bool> predicate = () => true;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        base.RunCommand(args);

        Protein protein = args[kProtein] as Protein;
        float angle = (float)args[kAngle];

        Vector2 currentPosition = protein.Position;
        float currentRotation = protein.Rotation;

        Vector2 targetPosition = transform.position;
        float targetRotation = transform.localEulerAngles.z + angle;

        float time = 0;

        while (time < commandLength && predicate())
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

public class MoveProteinToTargetCommand : MoveProteinToRectTransformCommand
{
    public string target;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        if (!PointsOfInterest.TryGet(target, out transform)) yield break;

        yield return base.RunCommand(args);
    }
}

public class MoveProteinToTargetButBreakInRangeCommand : MoveProteinToRectTransformCommand
{
    public string target;
    public string point;
    public float range = 100;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        if (!PointsOfInterest.TryGet(target, out transform)) yield break;
        if (!PointsOfInterest.TryGet(point, out RectTransform pointTransform)) yield break;
        Protein protein = args[kProtein] as Protein;

        predicate = () => Vector2.Distance(protein.Position, pointTransform.position) > range;

        yield return base.RunCommand(args);
    }
}

public class MoveProteinToRandomTransformCommand : MoveProteinToRectTransformCommand
{
    public string target;
    public float directionRange = 45;
    public float forwardRange = 360;
    public float minDistance = 100;
    public float maxDistance = 200;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        Protein protein = args[kProtein] as Protein;
        if (!PointsOfInterest.TryGet(target, out RectTransform targetTransform)) yield break;

        float directionAngle = targetTransform.localEulerAngles.z;
        float forwardAngle = protein.Rotation;

        float distance = Random.Range(minDistance, maxDistance);
        float direction = Random.Range(directionAngle - directionRange, directionAngle + directionRange);
        float forward = Random.Range(forwardAngle - forwardRange, forwardAngle + forwardRange);

        protein.extraTransform.position = new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * distance;
        protein.extraTransform.localEulerAngles = new Vector3(0, 0, forward);

        transform = protein.extraTransform;

        yield return base.RunCommand(args);
    }
}

public class OrientToAngleCommand : TimedCommand
{
    public float angle;
    public bool unclamped = false;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        base.RunCommand(args);

        Protein protein = args[kProtein] as Protein;
        //float angle = (float)args[kAngle];

        float currentAngle = protein.Rotation;
        float targetAngle = angle;

        // Normalize current angle [0, 360)
        currentAngle %= 360.0f;
        if (currentAngle < 0)
            currentAngle += 360;

        // Normalize target angle [0, 360)
        // IF needed
        if (!unclamped)
        {
            targetAngle %= 360.0f;
            if (targetAngle < 0)
                targetAngle += 360;
        }

        if (Mathf.Approximately(currentAngle, targetAngle))
            yield break;

        float time = 0;

        while (time < commandLength)
        {
            float t = time / commandLength;

            // if animation curve is null, default to smooth step
            t = TimeCurve(t);

            protein.Rotation = Mathf.Lerp(currentAngle, targetAngle, t);

            time += Time.deltaTime;
            yield return null;
        }
    }
}

// ---===============---
// --- Util Commands ---
// ---===============---

public class BreakWhenInRangeCommand : AnimationCommand
{
    public string targetPoint;
    public float range = 100;
    [XmlElement(Type = typeof(MoveProteinToTargetCommand))]
    [XmlElement(Type = typeof(MoveProteinToRandomTransformCommand))]
    [XmlElement(Type = typeof(OrientToAngleCommand))]
    [XmlElement(Type = typeof(FullResetCommand))]
    public AnimationCommand commandToRun;

    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        Protein protein = args[kProtein] as Protein;
        if (!PointsOfInterest.TryGet(targetPoint, out RectTransform transform)) yield break;
        IEnumerator command = commandToRun.RunCommand(args);

        while (Vector2.Distance(protein.Position, transform.position) > range && command.MoveNext())
        {
            yield return command.Current;
        }
    }
}

public class FullResetCommand : AnimationCommand
{
    public override IEnumerator RunCommand(Dictionary<string, object> args)
    {
        Protein protein = args[kProtein] as Protein;
        protein.FullReset();
        yield break;
    }
}
