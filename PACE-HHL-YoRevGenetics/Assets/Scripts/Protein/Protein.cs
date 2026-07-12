using FAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Application = FAST.Application;
using Random = UnityEngine.Random;

public class Protein : MonoBehaviour
{
    public enum State
    {
        Idle,
        MidAction,
        PostAction,
    }
    State currentState = State.Idle;

    [Header("Events")]
    public UnityEvent<string> onProteinActionStarted;
    public UnityEvent onProteinActionDone;
    public UnityEvent onFullReset;

    [Header("General Animations")]
    [SerializeField] float animationLength = 1;
    [SerializeField] float animationWaitTime = 0.25f;
    [SerializeField] float animationEndTime = 3.0f;

    [Header("Bounce Animation")]
    [SerializeField] float bubbleSize = 100;
    [SerializeField] Vector2 randDistance = new(100, 200);
    [SerializeField] float randDirection = 45;
    [SerializeField] float randForward = 360;

    [Header("References")]
    [SerializeField] ProteinPiece proteinPiecePrefab;
    [SerializeField] Transform proteinPieceParent;

    RectTransform extraTransform;

    Dictionary<string, object> commandArgs;

    List<ProteinPiece> _proteinPieces = new();
    public List<ProteinPiece> ProteinPieces { get => _proteinPieces; private set => _proteinPieces = value; }
    public bool ProteinContainsPiece(string pieceName)
    {
        return ProteinPieces.Exists(p => p.LogicBlock.pieceName.Equals(pieceName));
    }

    Vector2? homePosition;
    float? homeRotation;

    public float Rotation
    {
        get => transform.localEulerAngles.z;
        set => transform.localEulerAngles = new(transform.localEulerAngles.x, transform.localEulerAngles.y, value);
    }

    public Vector2 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    private void Start()
    {
        var temp = new GameObject("extraTransform");
        extraTransform = temp.AddComponent<RectTransform>();

        PopulateArgDictionary();
    }

    public void OnInteractButton()
    {
        switch (currentState)
        {
            case State.Idle:
                DoAction();
                break;

            case State.MidAction:
                // do nothing
                break;

            case State.PostAction:
                FullReset();
                break;
        }
    }

    // ---==========---
    //  Action related
    // ---==========---
    private void PopulateArgDictionary()
    {
        commandArgs = new()
        {
            { AnimationCommand.kProtein, this },
            { AnimationCommand.kAngle, 0.0f },
        };
    }

    private void DoAction()
    {
        var (action, angle) = ResolveAction();

        if (string.IsNullOrEmpty(action))
        {
            Debug.LogError("No action given to perform!");
            return;
        }

        // Old (hardcoded) animation system
        //StartCoroutine(DoActionCoroutine(action, angle));

        if (!Application.settings.proteinAnimations.TryGetValue(action.ToLower(), out ProteinAnimation animation))
        {
            Debug.LogError($"Action '{action}' could not be found in {Application.skin}-proteinAnimations.xml");
            return;
        }
        StartCoroutine(DoProteinAnimation(animation, angle));
    }

    private IEnumerator DoProteinAnimation(ProteinAnimation animation, float? angle)
    {
        currentState = State.MidAction;
        onProteinActionStarted?.Invoke(animation.name);

        if (angle.HasValue)
        {
            yield return OrientToAngle(angle.Value, animationLength);
            yield return new WaitForSeconds(animationWaitTime);
        }

        commandArgs[AnimationCommand.kAngle] = angle ?? 0;

        // Run each command sequentially
        foreach (AnimationCommand command in animation.animationCommands)
        {
            yield return command.RunCommand(commandArgs);
        }

        yield return new WaitForSeconds(animationEndTime);

        currentState = State.PostAction;
        onProteinActionDone?.Invoke();
    }

    private IEnumerator DoActionCoroutine(string action, float? angle)
    {
        currentState = State.MidAction;
        onProteinActionStarted?.Invoke(action);

        if (angle.HasValue)
        {
            yield return OrientToAngle(angle.Value, animationLength);
            yield return new WaitForSeconds(animationWaitTime);
        }

        float realAngle = angle ?? 0;

        if (!Application.settings.pointsOfInterest.TryGetValue(PointsOfInterest.kProteinWinSpot, out RectTransform target))
        {
            Debug.LogError($"'{PointsOfInterest.kProteinWinSpot}' is not set but it is expected!");
            yield break;
        }

        switch (action)
        {
            case "win":
                yield return MoveToRectTransformWithAngle(target, realAngle, animationLength);
                break;

            case "kill":
                yield return MoveToRectTransformWithAngle(target, realAngle, animationLength, EaseInCubic);
                break;

            case "fail":
            case "bounce":
                if (Application.settings.pointsOfInterest.TryGetValue(PointsOfInterest.kBounceSpot, out RectTransform bounceSpot))
                {
                    yield return BreakWhenInRange(bounceSpot.position, bubbleSize,
                        MoveToRectTransformWithAngle(target, realAngle, animationLength));
                    RectTransform bouncedTransform = RandomRectTransform(bounceSpot.localEulerAngles.z, randDirection, Rotation, randForward, randDistance.x, randDistance.y);
                    yield return MoveToRectTransform(bouncedTransform, animationLength, EaseOutCubic);
                }
                break;

            case "separate":
                yield return OrientToAngleUnclamped(360*4, animationLength, EaseInCubic);
                FullReset();
                yield break;

            default:
                Debug.LogError($"Action '{action}' not implemented! (this is case-sensitive)");
                break;
        }

        yield return new WaitForSeconds(animationEndTime);

        currentState = State.PostAction;
        onProteinActionDone?.Invoke();
    }

    private IEnumerator MoveToRectTransform(RectTransform transform, float animationLength, Func<float, float> animationCurve = null) =>
        MoveToRectTransformWithAngle(transform, Rotation, animationLength, animationCurve);

    private IEnumerator MoveToRectTransformWithAngle(RectTransform transform, float angle, float animationLength, Func<float, float> animationCurve = null)
    {
        if (animationLength <= 0)
        {
            Debug.LogError("animationLength must be > 0!");
            yield break;
        }

        Vector2 currentPosition = Position;
        float currentRotation = Rotation;

        Vector2 targetPosition = transform.position;
        float targetRotation = transform.localEulerAngles.z + angle;

        float time = 0;

        while (time < animationLength)
        {
            float t = time / animationLength;

            // if animation curve is null, default to smooth step
            animationCurve ??= SmoothStep;
            t = animationCurve(t);

            Rotation = Mathf.LerpAngle(currentRotation, targetRotation, t);
            Position = Vector2.Lerp(currentPosition, targetPosition, t);

            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator OrientToAngleUnclamped(float angle, float animationLength, Func<float, float> animationCurve = null)
    {
        float currentAngle = Rotation;
        float targetAngle = angle;

        currentAngle %= 360.0f;
        if (currentAngle < 0)
            currentAngle += 360;

        if (Mathf.Approximately(currentAngle, targetAngle))
            yield break;

        float time = 0;

        while (time < animationLength)
        {
            float t = time / animationLength;

            // if animation curve is null, default to smooth step
            animationCurve ??= SmoothStep;
            t = animationCurve(t);

            Rotation = Mathf.Lerp(currentAngle, targetAngle, t);

            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator OrientToAngle(float angle, float animationLength, Func<float, float> animationCurve = null)
    {
        float currentAngle = Rotation;
        float targetAngle = angle;

        currentAngle %= 360.0f;
        if (currentAngle < 0)
            currentAngle += 360;

        targetAngle %= 360.0f;
        if (targetAngle < 0)
            targetAngle += 360;

        if (Mathf.Approximately(currentAngle, targetAngle))
            yield break;

        float time = 0;

        while (time < animationLength)
        {
            float t = time / animationLength;

            // if animation curve is null, default to smooth step
            animationCurve ??= SmoothStep;
            t = animationCurve(t);

            Rotation = Mathf.LerpAngle(currentAngle, targetAngle, t);

            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator BreakWhenInRange(Vector2 point, float range, IEnumerator action)
    {
        while (Vector2.Distance(Position, point) > range && action.MoveNext())
        {
            yield return action.Current;
        }
    }

    private RectTransform RandomRectTransform(float directionAngle, float directionRange, float forwardAngle, float forwardRange, float minDistance, float maxDistance)
    {
        float distance = Random.Range(minDistance, maxDistance);
        float direction = Random.Range(directionAngle - directionRange, directionAngle + directionRange);
        float forward = Random.Range(forwardAngle - forwardRange, forwardAngle + forwardRange);

        extraTransform.position = new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * distance;
        extraTransform.localEulerAngles = new Vector3(0, 0, forward);
        return extraTransform;
    }

    public (string action, float? angle) ResolveAction()
    {
        if (ProteinPieces == null || ProteinPieces.Count == 0)
        {
            Debug.LogWarning("Attempted to resolve action before protein built");
            return (null, null);
        }

        string action = null;
        float? angle = null;
        foreach (ProteinLogicBlock logicBlock in Application.settings.orderedProteinLogic)
        {
            if (ProteinContainsPiece(logicBlock.pieceName))
            {
                angle ??= logicBlock.angle;

                if (action == null)
                {
                    string newAction = ParseAction(logicBlock.action);
                    if (!string.IsNullOrEmpty(newAction))
                        action = newAction;
                }
            }
        }

        action ??= Application.settings.defaultAction;

        Debug.Log($"{action} at {(angle.HasValue ? angle.Value : "null")} degrees");
        return (action, angle);
    }

    private string ParseAction(string action)
    {
        string[] actions = action.Split(' ').Select(x => x.Trim()).ToArray();

        if (actions.Length == 1)
            return actions[0];

        if (actions.Length != 6)
        {
            Debug.LogError($"Expected 6 words in action but got {actions.Length} ('{action}')");
            return null;
        }

        if (!actions[0].Equals("IF"))
        {
            Debug.LogError($"Expected 1st word to be 'IF', instead read '{actions[0]}' (case-sensitive)");
            return null;
        }

        if (!actions[2].Equals("THEN"))
        {
            Debug.LogError($"Expected 3rd word to be 'THEN', instead read '{actions[2]}' (case-sensitive)");
            return null;
        }

        if (!actions[4].Equals("ELSE"))
        {
            Debug.LogError($"Expected 5th word to be 'ELSE', instead read '{actions[4]}' (case-sensitive)");
            return null;
        }

        if (ProteinContainsPiece(actions[1]))
            return actions[3];
        else
            return actions[5];
    }

    // ---==================---
    //  Events & Extra Publics
    // ---==================---
    public void OnProteinPieceNamesRecieved(List<string> logicBlockNames)
    {
        foreach (string logicBlockName in logicBlockNames)
        {
            ProteinPiece piece = Instantiate(proteinPiecePrefab, proteinPieceParent);
            piece.SetLogicBlock(logicBlockName);
            ProteinPieces.Add(piece);
        }
    }

    public void FullReset()
    {
        ResetPieces();
        BackToHome();

        onFullReset?.Invoke();
    }

    public void ResetPieces()
    {
        foreach (ProteinPiece piece in ProteinPieces)
        {
            Destroy(piece.gameObject);
        }
        ProteinPieces.Clear();
    }

    public void BackToHome()
    {
        if (homePosition == null)
            homePosition = Position;
        else
            Position = homePosition.Value;

        if (homeRotation == null)
            homeRotation = Rotation;
        else
            Rotation = homeRotation.Value;

        currentState = State.Idle;
    }

    // ---==========---
    //  Util Shorthand
    // ---==========---

    // All of these expect domain and range of [0, 1]

    private float SmoothStep(float x) => Mathf.SmoothStep(0.0f, 1.0f, x);

    private float Linear(float x) => x;

    private float EaseInCubic(float x) => x * x * x;

    private float EaseOutCubic(float x) { float u = (1 - x); return 1 - u * u * u; }

    // ---===---
    //  Gizmos!
    // ---===---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Position, bubbleSize);
    }
}
