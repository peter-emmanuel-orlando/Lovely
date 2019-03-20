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
        var newProjectile = GameObject.Instantiate<GameObject>(_PrefabPool.GetPrefab(ProjectileStats.prefabName).GameObject).GetComponent<Projectile>();
        newProjectile.initiator = performer.gameObject;
        newProjectile.transform.parent = SpawnedProjectileNewParent;
        newProjectile.transform.position = SpawnedProjectileNewLocation;
        newProjectile.transform.rotation = SpawnedProjectileNewRotation;
        newProjectile.enabled = true;
        activeProjectiles.Add(newProjectile);
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