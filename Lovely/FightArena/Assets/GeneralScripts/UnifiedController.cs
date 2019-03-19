using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine;

/// <summary>
/// animator goes into navAgent. navAgent goes into rigidbody. If gameobject
/// is under the influence of physics, then bypass navAgent
/// </summary>
public class UnifiedController : MonoBehaviour
{
    [SerializeField]
    public Avatar avatar;  
    
    [SerializeField]
    public AnimatorOverrideController overrideController;

    NavMeshAgent navAgent;
    Animator anim;
    Rigidbody rb;


    float lookHMaxSpeed = 300;//deg per sec
    float lookVMaxSpeed = 300;
    float RunSpeed { get { return overrideController["DefaultMale|RunForward"].averageSpeed.z; } }
    float WalkSpeed { get { return overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }
    float WalkBackSpeed { get { return -overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }
    float WalkSideSpeed { get { return 5f; } }//overrideController["Race:Gender|AnimationName"].averageSpeed.z; } }

    Vector3 animMovement;
    Vector3 navDestination;
    float deltaDegreesH;
    float deltaDegreesV;
    
    bool stayOnNavMesh = false;
    bool playMirrored = false;
    AnimationClip playAnimationNext;
    float holdAnimationTimeLength = 0.2f;
    float holdAnimationUntill = 0f;
    bool isLocked = false;

    readonly List<Collider> hurtBoxes = new List<Collider>();
    readonly Dictionary<HitBoxType, Collider> hitBoxes = new Dictionary<HitBoxType, Collider>();
    
    enum ControlMode
    { AnimNav = 0,  Navigation, AnimatedRoot, Physics }
    ControlMode movementSource = ControlMode.AnimNav;


    RuntimeAnimatorController BaseController { get { return overrideController.runtimeAnimatorController; } }
    //string currentAnimatorState { get { anim.GetAnimatorTransitionInfo(0).IsName("flight -> shot") } }



    protected virtual void Awake()
    {
        if (avatar != null && overrideController != null) Initialize();
    }

    protected void Initialize()
    {
        holdAnimationUntill = Time.time;

        foreach (var collider in transform.GetComponentsInChildren<Collider>())
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
            {
                hurtBoxes.Add(collider);
            }
            else if (collider.gameObject.layer == LayerMask.NameToLayer("HitBox"))
            {
                collider.enabled = false;
                switch (collider.gameObject.name)
                {
                    case "hand.R":
                        hitBoxes.Add(HitBoxType.HandR, collider);
                        break;
                    case "hand.L":
                        hitBoxes.Add(HitBoxType.HandL, collider);
                        break;
                    case "foot.R":
                        hitBoxes.Add(HitBoxType.FootR, collider);
                        break;
                    case "foot.L":
                        hitBoxes.Add(HitBoxType.FootL, collider);
                        break;
                    default:
                        break;
                }
            }
        }

        navAgent = transform.GetComponent<NavMeshAgent>();
        if (navAgent == null) navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.hideFlags = HideFlags.HideInInspector;
        navAgent.updatePosition = false;
        navDestination = transform.position;

        anim = transform.GetComponent<Animator>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        anim.hideFlags = HideFlags.HideInInspector;
        anim.avatar = avatar;
        var overrideInstance = new AnimatorOverrideController(BaseController);
        var tmp = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(tmp);
        overrideInstance.ApplyOverrides(tmp);
        overrideController = overrideInstance;
        anim.runtimeAnimatorController = overrideController;
        anim.applyRootMotion = false;

        rb = transform.GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.hideFlags = HideFlags.HideInInspector;
        rb.isKinematic = true;
        rb.useGravity = true;
    }

    protected virtual void Update()
    {
        SyncAnimation();
        SyncNavigation();
        SyncPhysics();
    }

    private void SyncAnimation()
    {
        if (movementSource == ControlMode.AnimNav)
        {
            //if state isnt movement, go to movement
            //set animator parameters here for physics or navigation
            anim.SetFloat("SpeedHorizontal", animMovement.x);
            anim.SetFloat("SpeedForward", animMovement.z);
            //rotating the camera and rotation of the body is handled manually outside of the animation system
            //looking up/down or left/right is the domain of the animation system
            transform.localRotation = transform.localRotation * Quaternion.Euler(0, deltaDegreesH, 0);
            //only set lookV if it is between +- maxAngle;
            Quaternion temp = Camera.main.transform.localRotation * Quaternion.Euler(deltaDegreesV, 0, 0);
            if (Quaternion.Angle(Quaternion.identity, temp) < 90f)
                Camera.main.transform.localRotation = temp;
            var lookVerticalDegrees = -Vector3.SignedAngle(transform.forward, Camera.main.transform.forward, transform.right);
            anim.SetFloat("LookVertical", lookVerticalDegrees / 90f);
        }
        else if (movementSource == ControlMode.Navigation)
        {
            var localizedNormalized = transform.InverseTransformVector(GetNormalizedSpeed( navAgent.desiredVelocity));
            anim.SetFloat("SpeedHorizontal", localizedNormalized.x);
            anim.SetFloat("SpeedForward", localizedNormalized.z);
        }
        else if (movementSource == ControlMode.AnimatedRoot)
        {
            
            if(playAnimationNext != null && Time.time >= holdAnimationUntill)
            {
                // at animation end, needs to put in physics if not on nav mesh, or put on nav mesh
                holdAnimationUntill = Time.time + holdAnimationTimeLength;
                var placeHolder = "PlaceHolder_ActionA";
                var stateName = "ActionA";
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("ActionA"))
                {
                    placeHolder = "PlaceHolder_ActionB";
                    stateName = "ActionB";
                }
                overrideController[placeHolder] = playAnimationNext;
                anim.SetBool("PlayMirrored", playMirrored);
                anim.CrossFade(stateName, 0.01f, 0);
                
                //if not on navmesh, activate body collider

                playAnimationNext = null;
            }
            else if (playAnimationNext == null)
            {
                var isStateA = anim.GetCurrentAnimatorStateInfo(0).IsName("ActionA");
                var isStateB = anim.GetCurrentAnimatorStateInfo(0).IsName("ActionB");
                var isInTransition = anim.IsInTransition(0);
                if ( !isInTransition && !isStateA && !isStateB)
                {
                    var tmp = new NavMeshHit();
                    if (NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas))
                        movementSource = ControlMode.Navigation;
                    else
                        movementSource = ControlMode.Physics;
                }
            }
        }
    }

    private void SyncNavigation()
    {
        if(movementSource == ControlMode.AnimNav)
        {
            var tmp = new NavMeshHit();
            if (!NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas))
            {
                movementSource = ControlMode.Physics;
                return;
            }
            navAgent.Move(anim.deltaPosition);
        }
        else if(movementSource == ControlMode.Navigation)
        {
            var tmp = new NavMeshHit();
            if (!NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas ))
            {
                movementSource = ControlMode.Physics;
                return;
            }
            if (navAgent.remainingDistance > 0)
            {
                navAgent.isStopped = false;
                navAgent.destination = navDestination;
            }
            else
            {
                navAgent.isStopped = true;
                navDestination = transform.position;
            }
        }
        else if(movementSource == ControlMode.AnimatedRoot)
        {
            //navAgent.nextPosition = transform.position;
            if (stayOnNavMesh)
                navAgent.Move(transform.position);
            else
                navAgent.Warp(transform.position);
        }
    }

    private void SyncPhysics()
    {
        //certain actions(ie jump, fly, and other vertical movements) 
        //are too complex to be scripted with physics. Therefore follow root motion animation
        //physics (ie falling, ragdolling getting hit by certain things) is to complex to do everything via animation so turn off everything and just physics
        //navigation is a downer to script, its better to use the built in, no matter how troublesome

        if (movementSource == ControlMode.Navigation)
        {
            rb.MovePosition(navAgent.nextPosition);
        }
        else if(movementSource == ControlMode.AnimNav)
        {
            rb.MovePosition(navAgent.nextPosition);
        }
        else if (movementSource == ControlMode.AnimatedRoot)
        {
            if (stayOnNavMesh)
                rb.MovePosition(navAgent.nextPosition);
            else
                rb.MovePosition(rb.position + anim.deltaPosition);
        }
        else if(movementSource == ControlMode.Physics)
        {
            rb.isKinematic = false;
        }
    }

    //helper methods
    private Vector3 GetNormalizedSpeed(Vector3 speed)
    {
        var speedNormalized_Z = 0f;
        if (speed.z > 0)
            speedNormalized_Z = (speed.z < WalkSpeed) ? 0.5f * Mathf.InverseLerp(0, WalkSpeed, speed.z) : 0.5f + 0.5f * Mathf.InverseLerp(WalkSpeed, RunSpeed, speed.z);
        else
            speedNormalized_Z = -Mathf.InverseLerp(0, Mathf.Abs(WalkBackSpeed), Mathf.Abs(speed.z));

        var speedNormalized_X = Mathf.InverseLerp(0, Mathf.Abs(WalkSideSpeed), Mathf.Abs(speed.x));
        if (speed.x < 0) speedNormalized_X *= -1;

        return new Vector3(speedNormalized_X, 0, speedNormalized_Z);
    }


    protected virtual void ReceiveAnimationEvents(string message)
    {
        if (message == AnimationEventMessages.animationLock) SetLocked(true);
        if (message == AnimationEventMessages.animationUnlock) SetLocked(false);
    }

    private void SetLocked(bool isLocked)
    {
        //only look remains unlocked
        movementSource = ControlMode.AnimatedRoot;
        this.isLocked = isLocked;
    }

    //public methods

    public void Look(float horizontalSpeedNormalized, float verticalSpeedNormalized)
    {
        horizontalSpeedNormalized = Mathf.Clamp(horizontalSpeedNormalized, -1f, 1f);
        verticalSpeedNormalized = Mathf.Clamp(verticalSpeedNormalized, -1f, 1f);
        deltaDegreesH = horizontalSpeedNormalized * lookHMaxSpeed * Time.deltaTime;
        deltaDegreesV = verticalSpeedNormalized * lookVMaxSpeed * Time.deltaTime;
    }

    //the moement is the animated movement bound to the navmesh
    public void Move(float horizontalSpeedNormalized, float forwardSpeedNormalized)
    {
        if (isLocked) return;
        movementSource = ControlMode.AnimNav;

        //clear navigation
        navAgent.isStopped = true;

        navDestination = transform.position;
        horizontalSpeedNormalized = Mathf.Clamp(horizontalSpeedNormalized, -1f, 1f);
        forwardSpeedNormalized = Mathf.Clamp(forwardSpeedNormalized, -1f, 1f);
        animMovement = new Vector3(horizontalSpeedNormalized, 0, forwardSpeedNormalized);
    }

    public void MoveToDestination(Vector3 destination)
    {
        if (isLocked)
            return;
        movementSource = ControlMode.Navigation;
        navDestination = destination;
    }

    //unless stayOnNavMesh == true, use physics colliders while animating
    public void PlayAnimation(AnimationClip clip, bool remainOnNavMesh = true, bool playMirrored = false)
    {
        if (isLocked) return;

        this.playMirrored = playMirrored;

        //clear navigation
        navAgent.isStopped = true;
        navDestination = transform.position;

        movementSource = ControlMode.AnimatedRoot;
        playAnimationNext = clip;
    }
    
    public enum HitBoxType
    {
        None = 0, HandR, HandL, FootR, FootL, Weapons
    }

    public void SetHurtBoxActiveState(bool isActive)
    {
        foreach (var hurtBox in hurtBoxes)
        {
            hurtBox.enabled = isActive;
        }
    }

    public void SetHitBoxActiveState(HitBoxType hitBoxType, bool isActive)
    {
        if(hitBoxes.ContainsKey(hitBoxType))
            hitBoxes[hitBoxType].enabled = isActive;
    }
}


/*
 * 
 * All fighting games (and many other games) are just state comparison machines. 
 * To determine whether or not an attack registers, they look at the state of the game on each frame.
 *
 * Each attack has something called a hitbox, or an area of the attack that is capable 
 * of hitting the enemy. Each character has a hurtbox, or an area that is capable of being
 * hit. Every frame, the game looks to see if a hit-box overlaps a hurtbox. If it does, then 
 * it registers a hit. If it doesn’t, the move will whiff. If both characters’ hitboxes overlap both
 * characters’ hurtboxes, the moves will trade. Finally, if both hitboxes overlap each other but do not 
 * overlap hurtboxes, the moves will “clash” in certain anime style fighting games.
 * 
 * Hitboxes and hurtboxes are not mapped to characters’ exact bodies. Instead, they are drawn in a rough 
 * approximation around where the attack is hitting. If you want to give an attack more “priority” you
 * give it a bigger hitbox. This allows it to beat other moves with smaller hitboxes. Similarly, if you
 * want a character to be invincible during certain parts of a move, you can remove their hurtboxes 
 * during certain frames.
 * 
*/
