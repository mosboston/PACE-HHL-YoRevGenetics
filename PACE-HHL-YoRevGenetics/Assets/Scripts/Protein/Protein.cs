using FAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Application = FAST.Application;

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

    [Header("References")]
    [SerializeField] ProteinPiece proteinPiecePrefab;
    [SerializeField] Transform proteinPieceParent;

    // Animations
    public RectTransform extraTransform;
    Dictionary<string, object> commandArgs;
    ProteinAnimation startAnimation;
    ProteinAnimation endAnimation;

    // Protein pieces
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

        AnimationInit();
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
    private void AnimationInit()
    {
        commandArgs = new()
        {
            { AnimationCommand.kProtein, this },
            { AnimationCommand.kAngle, 0.0f },
        };

        startAnimation = new()
        {
            name = "Start",
            animationCommands = new()
            {
                new OrientToAngleCommand()
                {
                    commandLength = 1.0f,
                    useArgAngle = true,
                },
                new WaitCommand()
                {
                    waitLength = 0.25f
                }
            },
        };

        endAnimation = new()
        {
            name = "End",
            animationCommands = new()
            {
                new WaitCommand()
                {
                    waitLength = 0.75f
                }
            },
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

        commandArgs[AnimationCommand.kAngle] = angle ?? 0;

        if (angle.HasValue)
        {
            yield return startAnimation.PlayAnimation(commandArgs);
        }

        yield return animation.PlayAnimation(commandArgs);

        yield return endAnimation.PlayAnimation(commandArgs);

        currentState = State.PostAction;
        onProteinActionDone?.Invoke();
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
}
