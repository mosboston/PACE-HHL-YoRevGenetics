using FAST;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Application = FAST.Application;

public class Protein : MonoBehaviour
{
    [SerializeField] ProteinPiece proteinPiecePrefab;
    [SerializeField] Transform proteinPieceParent;

    List<ProteinPiece> _proteinPieces = new();
    public List<ProteinPiece> ProteinPieces { get => _proteinPieces; private set => _proteinPieces = value; }
    public bool ProteinContainsPiece(string pieceName)
    {
        return ProteinPieces.Exists(p => p.LogicBlock.pieceName.Equals(pieceName));
    }

    Vector3? homePosition;

    public float Rotation
    {
        get => transform.localEulerAngles.z;
        set => transform.localEulerAngles = new(transform.localEulerAngles.x, transform.localEulerAngles.y, value);
    }

    public void DoAction()
    {
        var (action, angle) = ResolveAction();

        if (string.IsNullOrEmpty(action))
        {
            Debug.LogError("No action given to perform!");
            return;
        }

        StartCoroutine(DoActionCoroutine(action, angle));
    }

    public void OrientTowardsWithAngle(float angle, Vector2 target, float animationLength)
    {
        StartCoroutine(OrientTowardsWithAngleCoroutine(angle, target, animationLength));
    }

    private IEnumerator DoActionCoroutine(string action, float? angle)
    {
        if (angle.HasValue)
        {
            if (Application.settings.pointsOfInterest.TryGetValue(PointsOfInterest.kBindingSite, out Vector2 target))
                yield return OrientTowardsWithAngleCoroutine(angle.Value, target, 1);
            else
                Debug.LogError($"Not orienting because '{PointsOfInterest.kBindingSite}' was not set");
        }

        switch (action)
        {
            default:
                Debug.LogError($"Action '{action}' not implemented! (this is case-sensitive)");
                break;
        }

        yield return null;
    }

    private IEnumerator OrientTowardsWithAngleCoroutine(float angle, Vector2 target, float animationLength)
    {
        Vector2 towardsTarget = (Vector2)transform.position - target;
        float angleToTarget = Vector2.SignedAngle(transform.right, towardsTarget);

        float currentAngle = Rotation;
        float targetAngle = Rotation - angleToTarget + angle;

        currentAngle %= 360.0f;
        targetAngle %= 360.0f;

        if (Mathf.Approximately(currentAngle, targetAngle))
            yield break;

        float time = 0;

        while (time < animationLength)
        {
            float t = time / animationLength;
            t = Mathf.SmoothStep(0.0f, 1.0f, t);
            Rotation = Mathf.LerpAngle(currentAngle, targetAngle, t);
            time += Time.deltaTime;
            yield return null;
        }
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

        Debug.Log(action);
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

    public void OnProteinPieceNamesRecieved(List<string> logicBlockNames)
    {
        foreach (string logicBlockName in logicBlockNames)
        {
            ProteinPiece piece = Instantiate(proteinPiecePrefab, proteinPieceParent);
            piece.SetLogicBlock(logicBlockName);
            ProteinPieces.Add(piece);
        }
    }

    public void ResetPieces()
    {
        foreach (ProteinPiece piece in ProteinPieces)
        {
            Destroy(piece.gameObject);
        }
        ProteinPieces.Clear();

        if (homePosition == null)
            homePosition = transform.position;
        else
            transform.position = homePosition.Value;
    }
}
