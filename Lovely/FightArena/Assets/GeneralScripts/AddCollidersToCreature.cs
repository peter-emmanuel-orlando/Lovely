

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AddCollidersToCreature : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    bool finalize = false;
    [SerializeField]
    bool cancel = false;
    [SerializeField]
    float scale = 1f;

    List<Collider> addedColliders = new List<Collider>();
    private void Update()
    {
        var root = transform.FindDeepChild("root");
        DestroyImmediate(root.GetComponent<Collider>());
        var physicsCollider = root.gameObject.AddComponent<BoxCollider>();
        physicsCollider.center = Vector3.up * 0.15f;//root is up-side-down
        physicsCollider.size = new Vector3(0.5f, 2, 0.25f);
        addedColliders.Add(physicsCollider);

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
                col.isTrigger = true;
                col.radius = 0.05f * scale;
                if (bone.name == "hips" || bone.name == "spine" || bone.name == "chest" || bone.name == "upperChest")
                    col.radius = 0.1f * scale;
                col.height = 0.3f * scale;
                col.center = new Vector3(0, 0.1f, 0) * scale;
                if (bone == handR || bone == handL || bone == footR || bone == footL)
                {
                    bone.gameObject.layer = LayerMask.NameToLayer("HitBox");
                    col.radius = 0.5f * scale;
                    col.height = 1f * scale;
                }
                else
                    bone.gameObject.layer = LayerMask.NameToLayer("HurtBox");
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
#endif
}

