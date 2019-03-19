using UnityEngine;

public abstract class Projectile : Spawnable
{

    [ShowOnly]
    public GameObject initiator;

    public abstract ProjectileStats ProjectileStats { get; }
    public override string PrefabName { get { return ProjectileStats.prefabName; } }
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
