using System.Collections.Generic;
using UnityEngine;

public class GravityPoint : MonoBehaviour
{
    //protected override ArbitraryGravity This { get { return this; } }

    //protected override Bounds Bounds { get { throw new System.NotImplementedException(); } }

    HashSet<Rigidbody> tracked = new HashSet<Rigidbody>();
    HashSet<UnifiedController> trackedController = new HashSet<UnifiedController>();

    float minDistance = 50f;
    float maxDistance = 100f;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
#endif
    [SerializeField]
    float gravityForce = 10f;
    SphereCollider trigger;

    private void Awake()
    {
        trigger = gameObject.GetComponent<SphereCollider>();
        if(!trigger)
            trigger = gameObject.AddComponent<SphereCollider>();
        trigger.hideFlags = HideFlags.NotEditable;
        trigger.isTrigger = true;
        trigger.radius = maxDistance;
    }

    private void OnDestroy()
    {
        if(trigger)
        {
            if (Application.isPlaying)
                Destroy(trigger);
            else
                DestroyImmediate(trigger);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.attachedRigidbody)
        {
            var unified = other.attachedRigidbody.GetComponent<UnifiedController>();

            if (unified && !trackedController.Contains(unified))
                trackedController.Add(unified);
            else if (!tracked.Contains(other.attachedRigidbody))
                tracked.Add(other.attachedRigidbody);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody)
        {
            var unified = other.attachedRigidbody.GetComponent<UnifiedController>();
            
            if (unified && trackedController.Contains(unified))
                trackedController.Remove(unified);
            else if (tracked.Contains(other.attachedRigidbody))
                tracked.Remove(other.attachedRigidbody);
        }
    }

    private void FixedUpdate()
    {
        var markedForRemovalRB = new List<Rigidbody>();
        var markedForRemovalUnified = new List<UnifiedController>();

        foreach (var rb in tracked)
        {
            if (rb)
                rb.AddForce(CalculateForce(rb.position), ForceMode.Acceleration);
            else
                markedForRemovalRB.Add(rb);
        }
        foreach (var unified in trackedController)
        {
            if (unified)
                unified.AddForce(CalculateForce(unified.transform.position), ForceMode.Acceleration);
            else
                markedForRemovalUnified.Add(unified);
        }

        foreach (var unified in markedForRemovalUnified)
            trackedController.Remove(unified);
        foreach (var rb in markedForRemovalRB)
            tracked.Remove(rb);
    }

    Vector3 CalculateForce(Vector3 position)
    {
        var dir = (transform.position - position).normalized;

        float force = 0f;
        var sqDistance = (position - transform.position).sqrMagnitude;
        if (sqDistance <= minDistance * minDistance)
        {
            force = gravityForce;
        }
        else
        {
            //figure out square falloff
            var distance = (position - transform.position).magnitude;
            var lerpFactor = (distance - minDistance) / (maxDistance - minDistance);
            force = Mathf.Lerp(gravityForce, 0, lerpFactor);
        }

        var resultForce = dir * force;
        return resultForce;
    }
}
