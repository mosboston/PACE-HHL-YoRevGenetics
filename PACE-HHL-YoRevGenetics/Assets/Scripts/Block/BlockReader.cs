using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class BlockReader : MonoBehaviour
{
    public UnityEvent onStartedReading;
    public UnityEvent<List<string>> onCompletedReading;

    List<Block> readBlocks = new();

    bool reading = false;

    Animator anim;
    static readonly int BlockReadHash = Animator.StringToHash("BlockRead");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out Block block))
            return;

        Debug.Log(block.blockName);

        if (readBlocks.Contains(block))
        {
            Debug.LogWarning("Attempted to read block that alraedy has been read");
            return;
        }

        if (anim != null)
            anim.Play(BlockReadHash);

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
                onCompletedReading?.Invoke(readBlocks.Select(b => b.blockName).ToList());
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
