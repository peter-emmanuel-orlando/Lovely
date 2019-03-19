using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ProjectileAttack : AnimatedAbility
{
    protected abstract ProjectileStats ProjectileStats { get; }

    protected abstract float[] NormalizedProjectileSpawnTimes { get; }
    protected abstract Vector3 SpawnedProjectileNewLocation { get; }
    protected abstract Quaternion SpawnedProjectileNewRotation { get; }
    protected abstract Transform SpawnedProjectileNewParent { get; }
    
    protected abstract ScheduledAction[] OtherScheduledActions { get; }
    protected override sealed ScheduledActionQueue ScheduledActions
    {
        get
        {
            var l = new List<ScheduledAction>();
            foreach (var time in NormalizedProjectileSpawnTimes)
            {
                l.Add(new ScheduledAction(time, SpawnProjectile));
            }
            return new ScheduledActionQueue(OtherScheduledActions, l.ToArray());
        }
    }

    protected readonly List<Projectile> activeProjectiles = new List<Projectile>();
    
    public ProjectileAttack(Body body) : base(body)
    {

    }
    protected virtual void SpawnProjectile()
    {
        var newProjectile = GameObject.Instantiate<GameObject>(_PrefabPool.GetPrefab(ProjectileStats.prefabName).gameObject).GetComponent<Projectile>();
        newProjectile.initiator = performer.gameObject;
        newProjectile.transform.parent = SpawnedProjectileNewParent;
        newProjectile.transform.position = SpawnedProjectileNewLocation;
        newProjectile.transform.rotation = SpawnedProjectileNewRotation;
        newProjectile.enabled = true;
        activeProjectiles.Add(newProjectile);
    }   

}










public abstract class Projectile : MonoBehaviour, ISpawnable
{
    [ShowOnly]
    public GameObject initiator;

    public abstract ProjectileStats ProjectileStats { get; }
    public string PrefabName { get { return ProjectileStats.prefabName; } }
    public event ColliderEventHandler HitEvent;

    protected abstract event ColliderEventHandler OnHitEvent;
    protected abstract void ApplyEffectsOnce(Body hitBody);
    protected abstract void ApplyEffectsEveryFrame(Body hitBody);
    protected abstract void OnEnable();
    //Projectiles are currently cast immidiantly upon enable, this may not be the desired behavior
    //protected abstract void CastProjectile();

    private CollisionTracker collisionTracker;

    protected virtual void Awake()
    {
        collisionTracker = new CollisionTracker(this);
        OnHitEvent += OnHit;
    }
    

    protected void ResetCollisionTracker()
    {
        collisionTracker.Reset();
    }

    private void OnHit(GameObject sender, ColliderEventArgs e)
    {
        var collider = e.collider;
        if (HitEvent != null)
            HitEvent(gameObject, new ColliderEventArgs(collider));

        var hitBody = collider.GetComponentInParent<Body>();
        if(hitBody != null)
        {
            if (!collisionTracker.HasBeenAffectedThisFrame(hitBody))
                ApplyEffectsEveryFrame(hitBody);
            if (!collisionTracker.HasBeenAffected(hitBody))
                ApplyEffectsOnce(hitBody);

            collisionTracker.MarkAsAffected(hitBody);
        }

    }
}

public class ProjectileStats
{
    public readonly string prefabName;

    public ProjectileStats(string prefabName)
    {
        this.prefabName = prefabName;
    }
}