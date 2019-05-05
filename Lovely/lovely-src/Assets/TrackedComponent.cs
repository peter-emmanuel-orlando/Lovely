using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     todo: untested!
/// </summary>
/// <typeparam name="T"></typeparam>
public static class TrackedComponent
{
    //the z component of the vector3's is not tracked. right now the quantity of points is size^3 but by essentially
    //flattening to 2d it can be size^2
    private static readonly Dictionary<Vector3, TypeStore<IBounded>> posToObjList = new Dictionary<Vector3, TypeStore<IBounded>>();
    private static readonly HashSet<IBounded> trackedObjects = new HashSet<IBounded>();
    private static float cellSize = 10f;
    private static int locUpdatesPerFrame = 5;
    private static int lastUpdatedIndex = 0;
    private static int lastUpdateFrameCount = 0;
    private static bool isUpdating = false;

    private static IEnumerator UpdateTracked()
    {
        while (true)
        {
            if (Time.frameCount > lastUpdateFrameCount)
            {
                for (int i = 0; i < trackedObjects.Count; i++)
                {
                    //todo, takes same item over and over
                    var item = trackedObjects.FirstOrDefault();
                    Untrack(item);
                    if (item != null)
                        Track(item);
                }
                lastUpdateFrameCount = Time.frameCount;
            }
            yield return null;
        }
    }

    //called like TrackedComponent<Resource>.Track(Resource m);
    //by specifying what kind of tracked component, itll only let you submit that type for tracking
    public static void Track<T>(T m) where T : IBounded
    {
        //if the gameobject is destroyed do nothing
        if (m == null) return;

        if (!isUpdating)
        {
            var master = _Master.MasterSingleton;
            master.StopCoroutine("UpdateTracked");
            master.StartCoroutine(UpdateTracked());
            isUpdating = true;
        }

        var roundedPoints = GetRoundedOverlapPoints(m.Bounds);

        Untrack(m);

        foreach (var roundedPos in roundedPoints)
        {
            if (!posToObjList.ContainsKey(roundedPos))
                posToObjList.Add(roundedPos, new TypeStore<IBounded>());
            posToObjList[roundedPos].Add(m);
        }
        trackedObjects.Add(m);
    }

    public static void Untrack<T>(T m) where T : IBounded
    {
        if (m != null && trackedObjects.Remove(m))
        {
            var mType = m.GetType();
            foreach (var roundedPos in GetRoundedOverlapPoints(m.Bounds))
            {
                if (posToObjList.ContainsKey(roundedPos))
                {
                    if (posToObjList[roundedPos].ContainsValue(m))
                        posToObjList[roundedPos].Remove(m);
                    if (posToObjList[roundedPos].Count == 0)
                        posToObjList.Remove(roundedPos);
                }
            }
        }
    }

    private static Vector3[] GetRoundedOverlapPoints(Bounds b)
    {
        var minPoint = GetRoundedPos(b.center - b.extents);
        var maxPoint = GetRoundedPos(b.center + b.extents);
        List<Vector3> points = new List<Vector3>();

        for (float i = minPoint.x; i <= maxPoint.x; i += cellSize)
        {
            for (float j = minPoint.y; j <= maxPoint.y; j += cellSize)
            {
                for (float k = minPoint.z; k <= maxPoint.z; k += cellSize)
                {
                    points.Add(new Vector3(i, j, k));
                    //DebugShape.DrawSphere(new Vector3(i, j, k), .5f, Color.green);
                }
            }
        }
        return points.ToArray();
    }

    private static Vector3 GetRoundedPos(Vector3 sourcePos)
    {
        var roundedPos = new Vector3(sourcePos.x - (sourcePos.x % cellSize), sourcePos.y - (sourcePos.y % cellSize), sourcePos.z - (sourcePos.z % cellSize));
        return roundedPos;
    }

    //sorted by distance
    public static IEnumerable<T> GetOverlapping<T>(Vector3 sourcePos, float radius, bool includeDerivedTypes) where T : IBounded
    {
        var bounds = new Bounds(sourcePos, Vector3.one * radius);
        foreach (var item in GetOverlapping<T>(bounds, includeDerivedTypes))
        {
            var closestPoint = item.Bounds.ClosestPoint(sourcePos);
            if ((closestPoint - sourcePos).sqrMagnitude < radius * radius + 0.01)
                yield return item;
        }
    }
    //sorted by distance
    public static IEnumerable<T> GetOverlapping<T>(Bounds b, bool includeDerivedTypes) where T : IBounded
    {
        var posList = GetRoundedOverlapPoints(b);
        var usedItems = new HashSet<T>();
        foreach (var roundedPos in posList)
        {
            if (posToObjList.ContainsKey(roundedPos))
            {
                foreach (var item in posToObjList[roundedPos].GetData<T>(includeDerivedTypes))
                {
                    if(!usedItems.Contains(item))
                    {
                        usedItems.Add(item);
                        yield return item;
                    }
                }
            }
        }
    }
}

/*
 * using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     todo: untested!
/// </summary>
/// <typeparam name="T"></typeparam>
public static class TrackedComponent<T> where T : MonoBehaviour, ISpawnable
{
    //the z component of the vector3's is not tracked. right now the quantity of points is size^3 but by essentially
    //flattening to 2d it can be size^2
    private static readonly Dictionary<Vector3, HashSet<T>> posToObjList = new Dictionary<Vector3, HashSet<T>>();
    private static readonly Dictionary<T, HashSet<Vector3>> objToPosList = new Dictionary<T, HashSet<Vector3>>();
    private static float cellSize = 10f;
    private static int locUpdatesPerFrame = 5;
    private static int lastUpdatedIndex = 0;
    private static int lastUpdateFrameCount = 0;
    private static bool isUpdating = false;

    private static IEnumerator UpdateTracked()
    {
        while (true)
        {
            if (Time.frameCount > lastUpdateFrameCount)
            {
                var a = objToPosList.Keys.ToArray();
                foreach (var item in a)
                {                        
                    Untrack(item);
                    if (item != null)
                        Track(item);
                }
                lastUpdateFrameCount = Time.frameCount;
            }
            yield return null;
        }
    }
    
    //called like TrackedComponent<Resource>.Track(Resource m);
    //by specifying what kind of tracked component, itll only let you submit that type for tracking
    public static void Track( T m)
    {
        //if the gameobject is destroyed do nothing
        if (m == null) return;

        if(!isUpdating)
        {
            var master = _Master.MasterSingleton;
            master.StopCoroutine("UpdateTracked");
            master.StartCoroutine(UpdateTracked());
            isUpdating = true;
        }

        var roundedPoints = GetRoundedOverlapPoints(m.Bounds);

        Untrack(m);

        foreach (var roundedPos in roundedPoints)
        {
            if (!posToObjList.ContainsKey(roundedPos))
                posToObjList.Add(roundedPos, new HashSet<T>());
            if (!posToObjList[roundedPos].Contains(m))
                posToObjList[roundedPos].Add(m);
        }

        objToPosList.Add(m, new HashSet<Vector3>(roundedPoints));
    }

    public static void Untrack(T m)
    {
        if (!objToPosList.ContainsKey(m)) return;

        foreach (var roundedPos in objToPosList[m])
        {
            if (posToObjList.ContainsKey(roundedPos))
            {
                if (posToObjList[roundedPos].Contains(m))
                    posToObjList[roundedPos].Remove(m);
                if (posToObjList[roundedPos].Count <= 0)
                    posToObjList.Remove(roundedPos);
            }
        }
        objToPosList.Remove(m);
    }



    private static Vector3[] GetRoundedOverlapPoints(Bounds b)
    {
        var minPoint = GetRoundedPos(b.center - b.extents);
        var maxPoint = GetRoundedPos(b.center + b.extents);
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
    }

    private static Vector3 GetRoundedPos(Vector3 sourcePos)
    {
        var roundedPos = new Vector3(sourcePos.x - (sourcePos.x % cellSize), sourcePos.y - (sourcePos.y % cellSize), sourcePos.z - (sourcePos.z % cellSize));
        return roundedPos;
    }








    //sorted by distance
    public static T[] GetOverlapping(Vector3 sourcePos, float radius)
    {
        var bounds = new Bounds(sourcePos, Vector3.one * radius);
        List<T> result = new List<T>();
        foreach (var item in GetOverlapping(bounds))
        {
            if (item.Bounds.Contains(sourcePos))
                result.Add(item);
        } 
        return result.ToArray();
    }
    //sorted by distance
    public static T[] GetOverlapping(Bounds b)
    {
        var posList = GetRoundedOverlapPoints(b);
        List<T> result = new List<T>();
        foreach (var roundedPos in posList)
        {
            if (posToObjList.ContainsKey(roundedPos))
            {
                foreach (var item in posToObjList[roundedPos])
                {
                    if (item.Bounds.Intersects(b))
                        result.Add(item);
                }
            }
        }
        return result.ToArray();
    }

    //Getinbounds
    ///getclosest(maxrange)
    ///getallinrange
}
 * */
