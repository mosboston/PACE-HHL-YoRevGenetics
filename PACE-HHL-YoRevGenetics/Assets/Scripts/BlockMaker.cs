using System.Collections.Generic;
using UnityEngine;

using Application = FAST.Application;

public class BlockMaker : MonoBehaviour
{
    [SerializeField] float blockSeparation = 200;
    [SerializeField] int blocksPerRow = 4;
    [SerializeField] private Block blockPrefab;

    private void Start()
    {
        List<string> proteinPieceNames = Application.settings.proteinPieceNames;

        for (int i = 0; i < proteinPieceNames.Count; i++)
        {
            Block newBlock = Instantiate(blockPrefab, transform);

            newBlock.SetName(proteinPieceNames[i]);

            float x = blockSeparation * (i % 4);
            float y = -blockSeparation * (i / 4);
            newBlock.transform.localPosition = new Vector3(x, y);
        }
    }
}
