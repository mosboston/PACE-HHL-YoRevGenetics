using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using FAST;

using Application = FAST.Application;


public class PointsOfInterest : MonoBehaviour
{
    [Serializable]
    public struct KeyPoint
    {
        public string key;
        public RectTransform point;
    }

    [SerializeField] List<KeyPoint> pointsOfInterest;

    public const string kBindingSite = "BindingSite";
    public const string kProteinWinSpot = "ProteinWinSpot";

    private void Start()
    {
        Dictionary<string, Vector2> actualPointsOfInterest = new();

        foreach (KeyPoint keyPoint in pointsOfInterest)
        {
            actualPointsOfInterest.Add(keyPoint.key, keyPoint.point.position);
        }

        if (!actualPointsOfInterest.ContainsKey(kBindingSite))
            Debug.LogError($"Expected '{kBindingSite}' to be a point of interest but it was not found!");

        Application.settings.pointsOfInterest = actualPointsOfInterest;
    }
}
