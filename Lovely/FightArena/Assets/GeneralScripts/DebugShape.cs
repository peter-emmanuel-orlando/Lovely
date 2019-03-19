using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugShape
{
    private static Mesh sphereMesh;
    private static Material debugMaterial;

    public static void DrawSphere(Vector3 center, float radius, Color color)
    {
        if(Application.isPlaying)
        {
            if(debugMaterial == null)
            {
                debugMaterial = new Material(Shader.Find("UCLA Game Lab/Wireframe/Single-Sided"));
            }

            if(sphereMesh == null)
            {
                var newDebug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereMesh = newDebug.GetComponent<MeshFilter>().mesh;
                GameObject.DestroyImmediate(newDebug);
            }


            var newTransform = new Matrix4x4();
            newTransform.SetTRS(center, Quaternion.identity, Vector3.one * radius);

            var modifiedMaterial = new Material(debugMaterial);
            modifiedMaterial.color = color;

            Graphics.DrawMesh(sphereMesh, newTransform, modifiedMaterial, 0);
        }
    }
}
