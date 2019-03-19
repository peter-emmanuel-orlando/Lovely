using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;



static class MyExtensions
{
    public static Vector3[] GetCornerVerticies(this Bounds bounds)
    {

        var center = bounds.center;
        var extents = bounds.extents;
        var cornerPositions = new Vector3[]
        {
            new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z),
            new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z),

            new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z),
            new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z),

            new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z),
            new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z),

            new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z),
            new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z),
        };

        return cornerPositions;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T Random<T>(this IList<T> list)
    {
        int k = ThreadSafeRandom.ThisThreadsRandom.Next(list.Count - 1);
        return list[k];
    }


    public static Texture2D GetRTPixels(this RenderTexture rt)
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
        return tex;
    }


    public static bool IsNaN(this Quaternion q)
    {
        return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
    }

    public static bool IsNaN(this Vector4 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsNaN(v.w);
    }

    public static bool IsNaN(this Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }

    public static bool IsNaN(this Vector2 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y);
    }

    










    //Breadth-first 
    public static Transform[] GetAllChildren(this Transform parent)//does not include parent itself, or its siblings
    {
        if (parent == null) return new Transform[] { };
        Transform[] result = new Transform[parent.childCount];
        if (parent.childCount == 0)
        {
            return result;
        }
        else
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                result[i] = parent.GetChild(i);
            }
        }


        foreach (Transform child in parent)
        {
            Transform[] tmp = child.GetAllChildren();
            if (tmp.Length != 0)
            {
                //Debug.Log ("prev result.Length = " + result.Length);
                Array.Resize<Transform>(ref result, result.Length + tmp.Length);
                //Debug.Log ("result.Length = " + result.Length);
                ///Debug.Log ("tmp.Length = " + tmp.Length);
                tmp.CopyTo(result, result.Length - tmp.Length);
            }
        }
        return result;
    }

    //Breadth-first 
    public static Transform[] GetAllChildrenWithTag(this Transform parent, string tag)//does not include parent itself, or its siblings
    {

        Transform[] result = new Transform[parent.childCount];
        if (parent.childCount == 0)
        {
            return result;
        }
        else
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                result[i] = parent.GetChild(i);
            }
            List<Transform> tmpList = new List<Transform>();
            foreach (Transform t in result)
            {
                if (t.gameObject.CompareTag(tag))
                {
                    tmpList.Add(t);
                    //Debug.Log ("Just added " + t + ". t.tag = " + t.gameObject.tag + ", searchTag = " + tag);
                }
            }
            result = tmpList.ToArray();
        }


        foreach (Transform child in parent)
        {
            Transform[] tmp = child.GetAllChildrenWithTag(tag);
            if (tmp.Length != 0)
            {
                //Debug.Log ("prev result.Length = " + result.Length);
                Array.Resize<Transform>(ref result, result.Length + tmp.Length);
                //Debug.Log ("result.Length = " + result.Length);
                ///Debug.Log ("tmp.Length = " + tmp.Length);
                tmp.CopyTo(result, result.Length - tmp.Length);
            }
        }
        return result;
    }






    /* copied from http://answers.unity3d.com/questions/799429/transformfindstring-no-longer-finds-grandchild.html */
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }





    public static Vector3 GetRandomPositionOnMesh(this Mesh mesh, Transform transform)
    {
        var verticies = mesh.vertices;
        var triangles = mesh.triangles;
        int randInt = UnityEngine.Random.Range(0, mesh.triangles.Length - 3);
        randInt = randInt - (randInt % 3);//this gives the previous multiple of three, or in this case, the first vertex of the triangle. +1 and +2 are then the next 2 vertecies
        Vector3 pos1 = transform.TransformPoint(verticies[mesh.triangles[randInt]]);
        Vector3 pos2 = transform.TransformPoint(verticies[mesh.triangles[randInt + 1]]);
        Vector3 pos3 = transform.TransformPoint(verticies[mesh.triangles[randInt + 2]]);
        Vector3 finalPos = Vector3.Lerp(pos1, pos2, UnityEngine.Random.value);
        finalPos = Vector3.Lerp(finalPos, pos3, UnityEngine.Random.value);
        Debug.DrawLine(pos1, finalPos, Color.magenta, 20);
        Debug.DrawLine(pos2, finalPos, Color.green, 20);
        Debug.DrawLine(pos3, finalPos, Color.blue, 20);
        Debug.DrawRay(finalPos, Vector3.up * 100, Color.white, 20);
        Debug.DrawLine(finalPos + Vector3.up * 0.1f, finalPos + Vector3.up * 2f, Color.yellow, 20);
        return finalPos;
    }
    

    public static bool GetSnapshot(this GameObject objectToRender, int imageWidth, int imageHeight, out Texture2D resultTexture)
    {
        //grab the main camera and mess with it for rendering the object - make sure orthographic 
        Camera cam = new GameObject().AddComponent<Camera>();
        //set the camera as disabled so we can control rendering manually
        //cam.enabled = false;
        cam.orthographic = true;
        cam.backgroundColor = Color.clear;
        cam.clearFlags = CameraClearFlags.Color;

        //texture for our camera to render into, dont forget to release!
        cam.targetTexture = new RenderTexture(imageWidth, imageHeight, 16);

        //render to screen rect area equal to out image size 
        cam.rect = new Rect(0, 0, imageWidth, imageHeight);
        //if there are no renderers on the object, then there is nothing to photograph, so continue
        var renderers = objectToRender.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) { resultTexture = Texture2D.blackTexture; return false; }
        //put gameobject to photograph far away from everthing else in a random place
        var prevTransform = objectToRender.transform.position;
        objectToRender.transform.position = new Vector3(UnityEngine.Random.Range(-1, 1), -1, UnityEngine.Random.Range(-1, 1)) * 100000f;// Random.onUnitSphere * 10000f;

        //grab size of object to render - place/size camera to fit 
        Bounds bb = renderers[0].bounds;//the first renderer
                                        //skip the first renderer, its already accounted for
        for (int i = 1; i < renderers.Length; i++)
        {
            bb.Encapsulate(renderers[i].bounds);
            //var newBounds = new Bounds();
            //newBounds.center = (bb.center + renderers[i].bounds.center) / (i + 1);
        }

        //place camera looking at centre of object - and backwards down the z-axis from it 
        cam.transform.position = new Vector3(bb.center.x, bb.center.y, bb.min.z);
        //make clip planes fairly optimal and enclose whole mesh 
        cam.nearClipPlane = 0.5f;
        cam.farClipPlane = 2 * bb.extents.z + 10f;//Mathf.Abs(cam.transform.position.z + 10f + bb.max.z);
                                                  //set camera size to just cover entire mesh 
        cam.orthographicSize = 1.01f * Mathf.Max(bb.extents.y, bb.extents.x);
        //cam.transform.position += Vector3.up * cam.orthographicSize * 0.05f;

        //get prev RenderTexture, then set our own as the active one
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        //actually do the rendering
        cam.Render();
        var tex = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBAHalf, false);
        // Read RenderTexture contents into a 2DTexture 
        tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        tex.Apply();
        // restore prev RenderTexture
        RenderTexture.active = currentRT;
        //turn all semi transparent pixels to fully opaque 
        for (int y = 0; y < imageHeight; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                var c = tex.GetPixel(x, y);
                if (c.r != 0 || c.g != 0 || c.b != 0) c.a = 1;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();

        //perform cleanup
        objectToRender.transform.position = prevTransform;
        cam.targetTexture.Release();
        if (!Application.isPlaying && Application.isEditor)
            GameObject.DestroyImmediate(cam.gameObject);
        else
            GameObject.Destroy(cam.gameObject);

        //return the textures
        resultTexture = tex;
        return true;
    }


}





public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(System.Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId))); }
    }
}