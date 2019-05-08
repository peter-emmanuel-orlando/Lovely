/* 
Make a billboard out of an object in the scene 
The camera will auto-place to get the best view of the object so no need for camera adjustment 

To use - place an object anywhere with any lighting you want. 
Add this script to your object. 
uncheck "wait" and you will get a snapshot of the object (looking down the +Z-axis at it) saved out to gameobject.name + billboard.png in your project folder 
background color will be transparent 
*/
using UnityEngine;
[ExecuteInEditMode]
public class GetSnapshotOfGameObject : MonoBehaviour
{
    [SerializeField]
    int imageWidth = 128;
    [SerializeField]
    int imageHeight = 128;
    [SerializeField]
    bool wait = true;

    //cue the picture
    public void Snap(int imageWidth = 128, int imageHeight = 128)
    {
        wait = false;
        this.imageWidth = imageWidth;
        this.imageHeight = imageHeight;
    }

    //can be optimized by hijacking the main camera then restoring it rather than creating a new camera
    void Update()
    {
        //we are waiting for the cue to take the snapshot
        if (wait) return;
        Texture2D tex;
        if(gameObject.GetSnapshot(imageWidth, imageHeight, out tex))
        {
            // Encode texture into PNG 
            var bytes = tex.EncodeToPNG();

            //if we are not playing, but are in the unity editor, DestroyImmediate
            if (!Application.isPlaying && Application.isEditor)
                DestroyImmediate(tex);
            else
                Destroy(tex);

            //check if there is resources folder and a snapshot folder, if not create it
            if (!System.IO.Directory.Exists(Application.dataPath + "/ObjectSnapshots"))
                System.IO.Directory.CreateDirectory(Application.dataPath + "/ObjectSnapshots");

            //check if file exists at path. If so, append increasing number
            string path = Application.dataPath + "/ObjectSnapshots/" + this.gameObject.name + "Billboard";
            if (System.IO.File.Exists(path + ".png"))
            {
                int addOn = 1;
                while (System.IO.File.Exists(path + " (" + addOn + ").png"))
                    addOn++;
                path = path + " (" + addOn + ")";
            }
            //write the texture
            System.IO.File.WriteAllBytes(path + ".png", bytes);
        }
        if (!Application.isPlaying && Application.isEditor)
            DestroyImmediate(this);
        else
            Destroy(this);
    }
    
}