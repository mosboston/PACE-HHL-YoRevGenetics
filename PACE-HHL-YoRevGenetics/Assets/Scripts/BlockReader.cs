using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class BlockReader : MonoBehaviour
{
    public static UnityAction onStartedReading;
    public static UnityAction onCompletedReading;

    List<Block> readBlocks;

    bool reading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out Block block))
            return;

        Debug.Log(block.blockName);

        if (block.IsStart)
        {
            if (!reading)
            {
                reading = true;
                onStartedReading?.Invoke();
            }
            else
            {
                Debug.LogWarning("Attempted to read start before end");
                return;
            }
        }
        else if (block.IsEnd)
        {
            if (reading)
            {
                reading = false;
                onCompletedReading?.Invoke();
                readBlocks.Clear();
            }
            else
            {
                Debug.LogWarning("Attempted to read end before start");
                return;
            }
        }
        else
        {
            if (reading)
            {
                readBlocks.Add(block);
            }
        }
    }
}
