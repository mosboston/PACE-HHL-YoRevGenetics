using UnityEngine;
using System.Collections.Generic;

public class Protein : MonoBehaviour
{
    [SerializeField] ProteinPiece proteinPiecePrefab;
    [SerializeField] Transform proteinPieceParent;

    List<ProteinPiece> _proteinPieces = new();
    public List<ProteinPiece> ProteinPieces { get => _proteinPieces; private set => _proteinPieces = value; }

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

    public void OnProteinBlocksRecieved(List<Block> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            ProteinPiece piece = Instantiate(proteinPiecePrefab, proteinPieceParent);
            piece.SetLogicBlock(blocks[i].blockName);
            piece.transform.SetAsFirstSibling();
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
