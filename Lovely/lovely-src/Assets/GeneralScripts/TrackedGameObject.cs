using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public abstract class TrackedGameObject<T> : MonoBehaviour where T : TrackedGameObject<T>
{
    private static readonly Dictionary<Vector3, HashSet<T>> inner = new Dictionary<Vector3, HashSet<T>>();
    private static readonly Queue<Action> locUpdateQueue = new Queue<Action>();
    //private static readonly Queue<Action> locUpdateNextQueue = new Queue<Action>();


    protected abstract T This { get; }
    protected abstract Bounds Bounds { get; }

    protected static float CellSize
    {
        get { return cellSize; }
        set
        {
            var correctedVal = Mathf.Clamp(value, 1, float.MaxValue);
            if (correctedVal != cellSize)
            {
                cellSize = correctedVal;
                ReacquireAll();
            }
        }
    }

    protected static int LocUpdatesPerFrame
    {
        get { return locUpdatesPerFrame; }
        set { locUpdatesPerFrame = Mathf.Clamp(value, 1, int.MaxValue); }
    }

    private static float cellSize = 10f;
    private static int locUpdatesPerFrame = 5;

    private Vector3[] prevRoundedCorners = new Vector3[8];

    private static bool hasUpdated = false;

    private void Awake()
    {
        OnEnable();
    }

    protected virtual void OnEnable()
    {
        Track();
    }

    protected virtual void Update()
    {
        hasUpdated = false;

    }

    protected virtual void LateUpdate()
    {
        if(!hasUpdated)
        {
            for (int i = 0; i < Mathf.Min(locUpdatesPerFrame, locUpdateQueue.Count) ; i++)
            {
                var trackAction = locUpdateQueue.Dequeue();
                trackAction();
            }

            hasUpdated = true;
        }
    }

    protected virtual void OnDisable()
    {
        Untrack();
    }

    private void Track()
    {
        //if the gameobject is destroyed do nothing
        if (this == null) return;

        locUpdateQueue.Enqueue(Track);

        var roundedPoints = GetRoundedOverlapPoints();

        Untrack();

        foreach (var roundedPos in roundedPoints)
        {
            var type = typeof(T);
            var inner = TrackedGameObject<T>.inner;
            if (!inner.ContainsKey(roundedPos))
                inner.Add(roundedPos, new HashSet<T>());
            if (!inner[roundedPos].Contains(This))
                inner[roundedPos].Add(This);
        }

        prevRoundedCorners = roundedPoints;
    }

    private Vector3[] GetRoundedOverlapPoints()
    {
        var minPoint = GetRoundedPos(Bounds.center - Bounds.extents);
        var maxPoint = GetRoundedPos(Bounds.center + Bounds.extents);
        List<Vector3> points = new List<Vector3>();

        for (float i = minPoint.x; i <= maxPoint.x; i += cellSize)
        {
            for (float j = minPoint.y; j <= maxPoint.y; j += cellSize)
            {
                for (float k = minPoint.z; k <= maxPoint.z; k += cellSize)
                {
                    points.Add(new Vector3(i, j, k));
                    //DebugShape.DrawSphere(new Vector3(i, j, k), .5f, Color.green, 1);
                }
            }
        }
        return points.ToArray();
        /*
        var cornerPositions = Bounds.GetCornerVerticies();
        var roundedCornerPositions = new Vector3[8];

        for (int i = 0; i < 8; i++)
        {
            var pos = cornerPositions[i];
            var roundedPos = GetRoundedPos(pos);
            roundedCornerPositions[i] = roundedPos;
        }

        return roundedCornerPositions;
        
    }

    private static Vector3 GetRoundedPos(Vector3 sourcePos)
    {
        var roundedPos = new Vector3(sourcePos.x - (sourcePos.x % cellSize), sourcePos.y - (sourcePos.y % cellSize), sourcePos.z - (sourcePos.z % cellSize));
        return roundedPos;
    }

    private void Untrack()
    {
        var inner = TrackedGameObject<T>.inner;
        foreach (var roundedPos in prevRoundedCorners)
        {
            if (roundedPos == null) continue;
            if (inner.ContainsKey(roundedPos))
            {
                if (inner[roundedPos].Contains(This))
                    inner[roundedPos].Remove(This);
                if (inner[roundedPos].Count <= 0)
                    inner.Remove(roundedPos);
            }
        }
    }

    private static void ReacquireAll()
    {

    }








    //sorted by distance
    public static T[] GetOverlapping(Vector3 sourcePos)
    {
        var roundedPos = GetRoundedPos(sourcePos);
        var inner = TrackedGameObject<T>.inner;
        List<T> result = new List<T>();
        if(inner.ContainsKey(roundedPos))
        {
            foreach (var item in inner[roundedPos])
            {
                if (item.Bounds.Contains(sourcePos))
                    result.Add(item);
            }
        }
        return result.ToArray();
    }

    //Getinbounds
    ///getclosest(maxrange)
    ///getallinrange
}*/