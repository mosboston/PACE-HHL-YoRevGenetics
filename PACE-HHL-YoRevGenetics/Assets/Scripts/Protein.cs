using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FAST;

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

    Vector3 homePosition;

    public float Rotation
    {
        get => transform.localEulerAngles.z;
        set => transform.localEulerAngles = new(transform.localEulerAngles.x, transform.localEulerAngles.y, value);
    }

    private void Awake()
    {
        homePosition = transform.position;
    }

    public void DoAction()
    {
        string action = ResolveAction();

        if (string.IsNullOrEmpty(action))
        {
            Debug.LogError("No action given to perform!");
            return;
        }
    }

    public string ResolveAction()
    {
        if (ProteinPieces == null || ProteinPieces.Count == 0)
        {
            Debug.LogWarning("Attempted to resolve action before protein built");
            return null;
        }

        string action = Application.settings.defaultAction;
        foreach (ProteinLogicBlock logicBlock in Application.settings.orderedProteinLogic)
        {
            if (ProteinContainsPiece(logicBlock.pieceName))
            {
                string newAction = ParseAction(logicBlock.action);

                if (!string.IsNullOrEmpty(newAction))
                {
                    action = newAction;
                    break;
                }
            }
        }

        Debug.Log(action);
        return action;
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
        transform.position = homePosition;
    }
}
