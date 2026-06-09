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

    public const string kProteinWinSpot = "ProteinWinSpot";
    public const string kBounceSpot = "BounceSpot";

    private void Start()
    {
        Dictionary<string, RectTransform> actualPointsOfInterest = new();

        foreach (KeyPoint keyPoint in pointsOfInterest)
        {
            actualPointsOfInterest.Add(keyPoint.key, keyPoint.point);
        }

        Application.settings.pointsOfInterest = actualPointsOfInterest;
    }
}
