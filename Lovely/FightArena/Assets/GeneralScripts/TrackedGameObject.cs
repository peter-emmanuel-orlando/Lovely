using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrackedGameObject<T> : TrackedGameObject //where T : TrackedGameObject<T>
{
    private static readonly Dictionary<Vector3, HashSet<GameObject>> inner = new Dictionary<Vector3, HashSet<GameObject>>();
    private static readonly Queue<Action> locUpdateQueue = new Queue<Action>();

    protected Vector3 Size
    {
        get { return bounds.size; }
        set
        {
            bounds = new Bounds(transform.position, value);
        }
    }

    private Bounds bounds = new Bounds();

    private const float cellSize = 10f;

    private Vector3[] prevRoundedCorners = new Vector3[8];

    private const int locUpdatesPerFrame = 5;
    private static bool hasUpdated = false;

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
        bounds.center = transform.position;

        var roundedCorners = GetRoundedCornerPositions();

        Untrack();

        foreach (var roundedPos in roundedCorners)
        {
            if (!inner.ContainsKey(roundedPos))
                inner.Add(roundedPos, new HashSet<GameObject>());
            if (!inner[roundedPos].Contains(gameObject))
                inner[roundedPos].Add(gameObject);
        }

        prevRoundedCorners = roundedCorners;
    }

    private Vector3[] GetRoundedCornerPositions()
    {
        var cornerPositions = bounds.GetCornerVerticies();
        var roundedCornerPositions = new Vector3[8];

        for (int i = 0; i < 8; i++)
        {
            var pos = cornerPositions[i];
            var roundedPos = new Vector3(pos.x - (pos.x % cellSize), pos.y - (pos.y % cellSize), pos.x - (pos.z % cellSize));
            roundedCornerPositions[i] = roundedPos;
        }

        return roundedCornerPositions;
    }

    private void Untrack()
    {

        foreach (var roundedPos in prevRoundedCorners)
        {
            if (roundedPos == null) continue;
            if (inner.ContainsKey(roundedPos))
            {
                if (inner[roundedPos].Contains(gameObject))
                    inner[roundedPos].Remove(gameObject);
                if (inner[roundedPos].Count <= 0)
                    inner.Remove(roundedPos);
            }
        }
    }
}

public abstract class TrackedGameObject : MonoBehaviour
{
	// Use this for initialization
	//void Start () {	}
	
	// Update is called once per frame
	//void Update () {	}
}
