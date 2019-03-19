#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AddCollidersToCreature : MonoBehaviour
{

    [SerializeField]
    bool finalize = false;
    [SerializeField]
    bool cancel = false;
    [SerializeField]
    float scale = 1f;

    List<Collider> addedColliders = new List<Collider>();
    private void Update()
    {
        var bones = transform.FindDeepChild("root").GetAllChildren();
        var handR = transform.FindDeepChild("hand.R");
        var handL = transform.FindDeepChild("hand.L");
        var footR = transform.FindDeepChild("foot.R");
        var footL = transform.FindDeepChild("foot.L");
        var eyeR = transform.FindDeepChild("Eye.R");
        var eyeL = transform.FindDeepChild("Eye.L");

        foreach (var bone  in bones)
        {
            if (
                (bone.IsChildOf(handL) && bone != handL) || 
                (bone.IsChildOf(handR) && bone != handR) ||
                (bone.IsChildOf(footL) && bone != footL)  ||
                (bone.IsChildOf(footR) && bone != footR)  ||
                bone == eyeR ||
                bone == eyeL
                )
            {
                continue;
            }
            else
            {
                DestroyImmediate( bone.GetComponent<Collider>());
                var col = bone.gameObject.AddComponent<CapsuleCollider>();
                addedColliders.Add(col);
                if (bone == handR || bone == handL || bone == footR || bone == footL)
                    bone.gameObject.layer = LayerMask.NameToLayer("HitBox");
                else
                    bone.gameObject.layer = LayerMask.NameToLayer("HurtBox");
                col.isTrigger = true;
                col.radius = 0.005f * scale;
                if (bone.name == "hips" || bone.name == "spine" || bone.name == "chest" || bone.name == "upperChest")
                    col.radius = 0.01f * scale;
                col.height = 0.03f * scale;
                col.center = new Vector3(0, 0.01f, 0) * scale;
            }
        }
        if(cancel)
        {
            for (int i = 0; i < addedColliders.Count; i++)
            {
                DestroyImmediate(addedColliders[i]);
            }
            Debug.Log("cancelled add colliders!");
            DestroyImmediate(this);
        }
        if(finalize)
        {
            Debug.Log("successfully added colliders!");
            DestroyImmediate(this);
        }

    }
}


#endif