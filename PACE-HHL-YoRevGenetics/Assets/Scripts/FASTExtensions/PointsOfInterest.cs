using System;
using System.Collections.Generic;
using UnityEngine;

using Application = FAST.Application;


public class PointsOfInterest : MonoBehaviour
{
    public static Dictionary<string, RectTransform> dict => Application.settings.pointsOfInterest;

    public static bool TryGet(string key, out RectTransform transform)
    {
        if (!dict.TryGetValue(key, out transform))
        {
            Debug.LogError($"Key of '{key}' can not be found as a point of interest!");
            return false;
        }

        return true;
    }

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
