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
public partial class UnifiedController : MonoBehaviour
{
    [SerializeField]
    public Avatar avatar;  
    
    [SerializeField]
    public AnimatorOverrideController overrideController;

    protected NavMeshAgent navAgent;
    protected Animator anim;
    protected Rigidbody rb;
    private Transform cameraBone;
    public Transform CameraBone { get { return cameraBone; } }
    public NavMeshAgent NavAgent { get { return navAgent; } }

    private float lookHMaxSpeed = 300;//deg per sec
    private float lookVMaxSpeed = 300;

    private float RunSpeed { get { return overrideController["DefaultMale|RunForward"].averageSpeed.z; } }

    private float WalkSpeed { get { return overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }

    private float WalkBackSpeed { get { return -overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }

    private float WalkSideSpeed { get { return 5f; } }//overrideController["Race:Gender|AnimationName"].averageSpeed.z; } }

    private Vector3 animMovement;
    private Vector3 navDestination;
    private bool jump = false;
    private RecoilCode recoil = RecoilCode.None;
    private float deltaDegreesH;
    private float deltaDegreesV;
    private bool stayOnNavMesh = false;
    private bool playMirrored = false;
    private AnimationClip playAnimationNext;
    private bool isLocked = false;
    private readonly List<CapsuleCollider> hurtBoxes = new List<CapsuleCollider>();
    private readonly Dictionary<HitBoxType, CapsuleCollider> hitBoxes = new Dictionary<HitBoxType, CapsuleCollider>();

    private enum ControlMode
    { AnimNav = 0,  Navigation, AnimatedRoot, Physics }

    private ControlMode movementSource = ControlMode.AnimNav;

    private RuntimeAnimatorController BaseController { get { return overrideController.runtimeAnimatorController; } }

    //string currentAnimatorState { get { anim.GetAnimatorTransitionInfo(0).IsName("flight -> shot") } }


    private bool IsInitialized = false;

    protected virtual void Awake()
    {
        if (avatar != null && overrideController != null) Initialize();
    }

    protected void Initialize()
    {
        IsInitialized = true;

        cameraBone = transform.FindDeepChild("cameraBone");
        if(cameraBone == null)
        {
            cameraBone = new GameObject("cameraBone").transform;
            cameraBone.SetParent(transform);
            cameraBone.localPosition = Vector3.zero;
        }

        foreach (var collider in transform.GetComponentsInChildren<CapsuleCollider>())
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
        if (!IsInitialized) return;
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
            Quaternion temp = cameraBone.localRotation * Quaternion.Euler(deltaDegreesV, 0, 0);
            if (Quaternion.Angle(Quaternion.identity, temp) < 90f)
                cameraBone.localRotation = temp;
            var lookVerticalDegrees = -Vector3.SignedAngle(transform.forward, cameraBone.forward, transform.right);
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
            if(recoil != RecoilCode.None)
            {
                isLocked = true;
                anim.CrossFade(Enum.GetName(typeof(RecoilCode), recoil), 0.01f, 0);
            }
            if (jump)
            {
                isLocked = true;
                var isStateJump = anim.GetCurrentAnimatorStateInfo(0).IsName("Jump");
                if(!isStateJump) anim.CrossFade("Jump", 0.01f, 0);
                jump = false;
            }            
            else if(playAnimationNext != null)
            {
                // at animation end, needs to put in physics if not on nav mesh, or put on nav mesh
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
            else 
            {
                var isInTransition = anim.IsInTransition(0);
                var isStateA = anim.GetCurrentAnimatorStateInfo(0).IsName("ActionA");
                var isStateB = anim.GetCurrentAnimatorStateInfo(0).IsName("ActionB");
                var isStateJump = anim.GetCurrentAnimatorStateInfo(0).IsName("Jump");

                if ( !isInTransition && !isStateA && !isStateB && !isStateJump)
                {
                    var tmp = new NavMeshHit();
                    if (NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas))
                    {
                        movementSource = ControlMode.Navigation;
                        isLocked = false;
                    }
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
            var tmp = new NavMeshHit();
            if (NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas))
            {
                movementSource = ControlMode.Navigation;
                navAgent.Warp(tmp.position);
                rb.isKinematic = true;
                isLocked = false;
            }
            else
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
        if (isLocked || !IsInitialized) return;

        //clear navigation
        navAgent.isStopped = true;
        navDestination = transform.position;
        //clear jump
        jump = false;
        //clear playAnimation
        playAnimationNext = null;
        //clear playAnimation(recoil)
        recoil = RecoilCode.None;

        //check if manual movement is viable
        if (!navAgent.isOnNavMesh)
        {
            if (navAgent.isOnOffMeshLink)
                movementSource = ControlMode.Navigation;
            else
                movementSource = ControlMode.Physics;

            return;
        }

        //set up for manual movement
        movementSource = ControlMode.AnimNav;
        horizontalSpeedNormalized = Mathf.Clamp(horizontalSpeedNormalized, -1f, 1f);
        forwardSpeedNormalized = Mathf.Clamp(forwardSpeedNormalized, -1f, 1f);
        animMovement = new Vector3(horizontalSpeedNormalized, 0, forwardSpeedNormalized);
    }

    public void MoveToDestination(Vector3 destination)
    {
        if (isLocked || !IsInitialized) return;

        //clear manual movement
        animMovement = Vector3.zero;
        //clear jump
        jump = false;
        //clear playAnimation
        playAnimationNext = null;
        //clear playAnimation(recoil)
        recoil = RecoilCode.None;

        movementSource = ControlMode.Navigation;
        navDestination = destination;
    }

    public void Jump()
    {
        if (isLocked || !IsInitialized) return;

        //clear manual movement
        animMovement = Vector3.zero;
        //clear navigation
        navAgent.isStopped = true;
        navDestination = transform.position;
        //clear playAnimation
        playAnimationNext = null;
        //clear playAnimation(recoil)
        recoil = RecoilCode.None;

        movementSource = ControlMode.AnimatedRoot;
        jump = true;
    }

    //unless stayOnNavMesh == true, use physics colliders while animating
    public IEnumerator PlayAnimation(AnimationClip clip, bool remainOnNavMesh = true, bool playMirrored = false)
    {
        var result = _PlayAnimation(clip, remainOnNavMesh, playMirrored);
        result.MoveNext();
        return result;
    }
    private IEnumerator _PlayAnimation(AnimationClip clip, bool remainOnNavMesh, bool playMirrored)
    { 
        if (isLocked || !IsInitialized || clip == null) yield break;

        //clear manual movement
        animMovement = Vector3.zero;
        //clear navigation
        navAgent.isStopped = true;
        navDestination = transform.position;
        //clear jump
        jump = false;
        //clear playAnimation(recoil)
        recoil = RecoilCode.None;

        movementSource = ControlMode.AnimatedRoot;
        this.playMirrored = playMirrored;
        playAnimationNext = clip;
        yield break;
    }

    public IEnumerator PlayAnimation(RecoilCode code, bool remainOnNavMesh = true)
    {
        var result = _PlayAnimation(code, remainOnNavMesh);
        result.MoveNext();
        return result;
    }
    private IEnumerator _PlayAnimation(RecoilCode code, bool remainOnNavMesh)
    {
        if (isLocked || !IsInitialized || code == RecoilCode.None) yield break;
        movementSource = ControlMode.AnimatedRoot;

        //clear manual movement
        animMovement = Vector3.zero;
        //clear navigation
        navAgent.isStopped = true;
        navDestination = transform.position;
        //clear jump
        jump = false;
        //clear playAnimation
        playAnimationNext = null;

        recoil = code;
        yield break;
    }

    public void SetHurtBoxActiveState(bool isActive)
    {
        if (!IsInitialized) return;
        foreach (var hurtBox in hurtBoxes)
        {
            hurtBox.enabled = isActive;
        }
    }

    public CapsuleCollider SetHitBoxActiveState(HitBoxType hitBoxType, bool isActive)
    {
        CapsuleCollider result = null;
        if (!IsInitialized) return result;
        if (hitBoxes.ContainsKey(hitBoxType))
        {
            result = hitBoxes[hitBoxType];
            hitBoxes[hitBoxType].enabled = isActive;
        }
        return result;
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



    
