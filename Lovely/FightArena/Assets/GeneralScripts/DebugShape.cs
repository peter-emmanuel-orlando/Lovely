using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugShape
{
    public static void DrawSphere(Vector3 center, float radius, Color color, float lifeTime)
    {
        if(Application.isPlaying)
        {
            var newDebug = GameObject.Instantiate<GameObject>(_PrefabPool.GetPrefab("DebugSphere").gameObject);
            newDebug.GetComponent<MeshRenderer>().material.color = color;
            newDebug.transform.position = center;
            newDebug.transform.localScale *= radius * 2f;
            GameObject.Destroy(newDebug, lifeTime);
        }
    }
}
