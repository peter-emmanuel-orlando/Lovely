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
public abstract class UnifiedController : MonoBehaviour
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

    private readonly float lookHMaxSpeed = 300;//deg per sec
    private readonly float lookVMaxSpeed = 300;

    private float RunSpeed { get { return overrideController["DefaultMale|RunForward"].averageSpeed.z; } }

    private float WalkSpeed { get { return overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }

    private float WalkBackSpeed { get { return -overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }

    private float WalkSideSpeed { get { return 5f; } }//overrideController["Race:Gender|AnimationName"].averageSpeed.z; } }
    //needs to come from the animations too
    protected Vector3 jumpVelocity = new Vector3(5, 5, 5);

    private Vector3 animMovement;
    private Vector3 navDestination;
    private bool jump = false;
    private RecoilCode recoil = RecoilCode.None;
    private float deltaDegreesH;
    private float deltaDegreesV;
    private bool remainOnNavMesh = false;
    private bool playMirrored = false;
    private AnimationClip playAnimationNext;
    private string playAnimationSlot = "ActionA";
    private IEnumerator<float> playAnimationEnumerator;
    PlayToken currentToken;
    private bool isLocked = false;
    private Collider physicsCollider;
    private Action additionalPhysics;
    private float requirePhysicsTimeLength = 0;
    Vector3 deltaPosForPhys = Vector3.zero;
    Quaternion deltaRotForPhys = Quaternion.identity;

    private enum ControlMode
    { AnimNav = 0, Navigation, AnimatedRoot, Physics }

    private ControlMode movementSource = ControlMode.AnimNav;


    private RuntimeAnimatorController BaseController { get { return overrideController.runtimeAnimatorController; } }

    public bool IsLocked
    {
        get
        {
            return isLocked;
        }
    }

    private bool IsInitialized = false;


    //*////////////////////////////////////////////////////////////////////////
    #region events and callbacks
    //---------------------------------------------------------------------------
    public event AwakeEventHandler AwakeEvent;
    public event UpdateEventHandler UpdateEvent;
    public event OnDestroyEventHandler OnDestroyEvent;

    public event ColliderEventHandler OnTriggerEnterEvent;
    public event ColliderEventHandler OnTriggerStayEvent;
    public event ColliderEventHandler OnTriggerExitEvent;


    protected virtual void Awake()
    {
        InnerAwake();
        if (AwakeEvent != null)
            AwakeEvent(this, new EventArgs());
    }
    protected virtual void Update()
    {
        InnerUpdate();
        if (UpdateEvent != null)
            UpdateEvent(this, new EventArgs());
    }
    protected virtual void OnDestroy()
    {
        if (OnDestroyEvent != null)
            OnDestroyEvent(this, new EventArgs());
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (OnTriggerEnterEvent != null)
            OnTriggerEnterEvent(gameObject, new ColliderEventArgs(other));
    }
    protected virtual void OnTriggerStay(Collider other)
    {
        if (OnTriggerStayEvent != null)
            OnTriggerStayEvent(gameObject, new ColliderEventArgs(other));
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (OnTriggerExitEvent != null)
            OnTriggerExitEvent(gameObject, new ColliderEventArgs(other));
    }

    #endregion
    //---------------------------------------------------------------
    //***************************************************************




    //todo get avatar and override controller from the prefab pool, or in this classes case from abstract avatar and abstract oCtrl which will
    //come from inheriting class


    private void InnerAwake()
    {
        //should throw error
        if (avatar != null && overrideController != null)
            Initialize();
        else
            throw new UnityException();
    }

    private void Initialize()
    {
        IsInitialized = true;

        physicsCollider = transform.FindDeepChild("root").GetComponent<Collider>();

        cameraBone = transform.FindDeepChild("cameraBone");
        if (cameraBone == null)
        {
            cameraBone = new GameObject("cameraBone").transform;
            cameraBone.SetParent(transform);
        }
        cameraBone.localPosition = Vector3.zero;
        cameraBone.localEulerAngles = Vector3.zero;


        navAgent = transform.GetComponent<NavMeshAgent>();
        if (navAgent == null) navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.hideFlags = HideFlags.NotEditable;
        //navAgent.hideFlags = HideFlags.HideInInspector;
        navAgent.updatePosition = false;
        //navAgent.updateRotation = false;
        navAgent.angularSpeed = lookHMaxSpeed;
        navAgent.speed = RunSpeed;//float.MaxValue;
        navDestination = transform.position;
        //TEMPORARY WORKAROUNT todo
        navAgent.areaMask &= ~(1 << NavMesh.GetAreaFromName("Jump"));

        anim = transform.GetComponent<Animator>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        anim.hideFlags = HideFlags.NotEditable;
        //anim.hideFlags = HideFlags.HideInInspector;
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
        rb.hideFlags = HideFlags.NotEditable;
        //rb.hideFlags = HideFlags.HideInInspector;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = true;
        rb.useGravity = true;
        rb.mass = 77.7f;
        rb.angularDrag = 50f;
        rb.drag = 1f;
        //rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void InnerUpdate()
    {
        if (!IsInitialized) return;
        /*
        var surf = TrackedGameObject<NavMeshSurfaceBase>.GetOverlapping(transform.position);
        if(surf.Length > 0)
        {
            var s = surf[0];
            navAgent.areaMask = 1 << NavMesh.GetAreaFromName("WalkableOther");
        }
        else
        {
            navAgent.areaMask = 1 << NavMesh.GetAreaFromName("WalkableMain");
        }*/
        SyncAnimation();
        SyncNavigation();
        SyncPhysics();
        //gameObject.DisplayTextComponent(this);
    }

    private void SyncAnimation()
    {
        if (playAnimationEnumerator != null && !playAnimationEnumerator.MoveNext())
            playAnimationEnumerator = null;


        if (movementSource != ControlMode.Navigation)
        {
            //handles look
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


        if (movementSource == ControlMode.AnimNav)
        {
            //if state isnt movement, go to movement
            //set animator parameters here for physics or navigation
            anim.SetFloat("SpeedHorizontal", animMovement.x);
            anim.SetFloat("SpeedForward", animMovement.z);
        }
        else if (movementSource == ControlMode.Navigation)
        {
            if (!navAgent.pathPending)//haspath?
            {
                var localizedNormalized = transform.InverseTransformVector(GetNormalizedSpeed(navAgent.velocity));//////////////
                anim.SetFloat("SpeedHorizontal", localizedNormalized.x);
                anim.SetFloat("SpeedForward", localizedNormalized.z);
            }
        }
        else if (movementSource == ControlMode.AnimatedRoot)
        {
            if (recoil != RecoilCode.None)
            {
                isLocked = true;
                anim.CrossFade(Enum.GetName(typeof(RecoilCode), recoil), 0.01f, 0);
            }
            else if (jump)
            {
                jump = false;
                isLocked = true;
                var isStateJump = anim.GetCurrentAnimatorStateInfo(0).IsName("Jump");
                if (!isStateJump) anim.CrossFade("Jump", 0.01f, 0);
                movementSource = ControlMode.Physics;
            }
            else if (playAnimationNext != null)
            {
                // at animation end, needs to put in physics if not on nav mesh, or put on nav mesh             
                overrideController["PlaceHolder_" + playAnimationSlot] = playAnimationNext;
                anim.SetBool("PlayMirrored", playMirrored);
                anim.CrossFadeInFixedTime(playAnimationSlot, 0.1f, 0);
                //if not on navmesh, activate body collider
                playAnimationNext = null;
            }
            else
            {
                var isInTransition = anim.IsInTransition(0);
                var isStateA = anim.GetCurrentAnimatorStateInfo(0).IsName("ActionA");
                var isStateB = anim.GetCurrentAnimatorStateInfo(0).IsName("ActionB");
                var isStateJump = anim.GetCurrentAnimatorStateInfo(0).IsName("Jump");

                if (!isInTransition && !isStateA && !isStateB && !isStateJump)
                {
                    currentToken = null;
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
        else if (movementSource == ControlMode.Physics)
        {
            anim.SetFloat("SpeedHorizontal", 0);
            anim.SetFloat("SpeedForward", 0);
        }
    }

    private void SyncNavigation()
    {

        if (movementSource == ControlMode.AnimNav)
        {
            var tmp = new NavMeshHit();
            if (!NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas))
            {
                movementSource = ControlMode.Physics;
                return;
            }
            navAgent.Move(anim.deltaPosition);
        }
        else if (movementSource == ControlMode.Navigation)
        {
            var tmp = new NavMeshHit();
            if (!navAgent.isOnOffMeshLink && !NavMesh.SamplePosition(transform.position, out tmp, 2f, NavMesh.AllAreas))
            {
                movementSource = ControlMode.Physics;
                return;
            }
            if (navAgent.remainingDistance > 0 || navAgent.destination != navDestination)
            {
                //warp the position and rotation to be matched up with rigidbody
                navAgent.isStopped = false;
                if (navAgent.destination != navDestination)
                    navAgent.destination = navDestination;
            }
            else
            {
                navAgent.isStopped = true;
                navDestination = transform.position;
            }
        }
        else if (movementSource == ControlMode.AnimatedRoot)
        {
            //navAgent.nextPosition = transform.position;
            if (remainOnNavMesh)
                navAgent.Move(anim.deltaPosition);
            else
                navAgent.Warp(transform.position);
        }
        else if (movementSource == ControlMode.Physics)
        {
            //navAgent.Warp(transform.position);
        }
    }

    private void SyncPhysics()
    {
        deltaPosForPhys = anim.deltaPosition;
        deltaRotForPhys = anim.deltaRotation;
    }

    private void FixedUpdate()
    {
        //navAgent.nextPosition = transform.position;
        //certain actions(ie jump, fly, and other vertical movements) 
        //are too complex to be scripted with physics. Therefore follow root motion animation
        //physics (ie falling, ragdolling getting hit by certain things) is to complex to do everything via animation so turn off everything and just physics
        //navigation is a downer to script, its better to use the built in, no matter how troublesome

        if (movementSource == ControlMode.Navigation)
        {
            rb.isKinematic = true;
            if (!navAgent.pathPending || navAgent.pathStatus == NavMeshPathStatus.PathPartial)//haspath?
            {
                //needs smoothing or something. if path is pending, it jitters while trying to get to nextpos
                rb.MovePosition(navAgent.nextPosition);
                //rb.MoveRotation(Quaternion.LookRotation(navAgent.desiredVelocity, transform.up));
            }
            //use a turntoface to update rotation to face navagent velocity
        }
        else if (movementSource == ControlMode.AnimNav)
        {
            rb.isKinematic = true;
            rb.MovePosition(navAgent.nextPosition);
        }
        else if (movementSource == ControlMode.AnimatedRoot)
        {
            if (remainOnNavMesh)
            {
                rb.isKinematic = true;
                rb.MovePosition(navAgent.nextPosition);
                rb.MoveRotation(rb.rotation * deltaRotForPhys);
            }
            else
            {
                rb.isKinematic = false;
                rb.velocity = (deltaPosForPhys) / Time.fixedDeltaTime;
                rb.angularDrag = 8f;
                // not yet working rb.angularVelocity = (anim.deltaRotation.eulerAngles) / Time.deltaTime;
                //rb.MoveRotation(rb.rotation * anim.deltaRotation);
                //rb.MovePosition(rb.position + anim.deltaPosition);
            }
        }
        else if (movementSource == ControlMode.Physics)
        {
            var rootPos = anim.rootPosition;
            Debug.unityLogger.logEnabled = false;
            var centerPos = anim.bodyPosition;
            Debug.unityLogger.logEnabled = true;
            var headPos = centerPos + (centerPos - rootPos);
            Debug.DrawLine(centerPos, rootPos, Color.red);
            Debug.DrawLine(centerPos, headPos, Color.magenta);
            var lowerCenter = (rootPos + centerPos) / 2f;
            var upperCenter = (headPos + centerPos) / 2f;
            Debug.DrawLine(centerPos, lowerCenter, Color.blue);
            Debug.DrawLine(centerPos, upperCenter, Color.cyan);
            var samplePositionRadius = Vector3.Distance(centerPos, lowerCenter);
            samplePositionRadius *= 2f;
            DebugShape.DrawSphere(lowerCenter, samplePositionRadius, Color.yellow);
            DebugShape.DrawSphere(headPos, samplePositionRadius, Color.green);

            rb.isKinematic = false;

            if (additionalPhysics != null)
            {
                additionalPhysics();
            }
            additionalPhysics = null;

            if(requirePhysicsTimeLength > 0)
            {
                requirePhysicsTimeLength -= Time.fixedDeltaTime;
            }
            else
            {
                var tmp = new NavMeshHit();
                if (NavMesh.SamplePosition(lowerCenter, out tmp, samplePositionRadius, NavMesh.AllAreas))
                {
                    movementSource = ControlMode.Navigation;
                    navAgent.Warp(tmp.position);
                    rb.isKinematic = true;
                    //rb.freezeRotation = true;
                    isLocked = false;
                }
                else if ( NavMesh.SamplePosition(headPos, out tmp, samplePositionRadius, NavMesh.AllAreas))
                {
                    var grabPoint = tmp.position;
                    if(NavMesh.FindClosestEdge(grabPoint, out tmp, NavMesh.AllAreas) && Vector3.SqrMagnitude(tmp.position - grabPoint) < samplePositionRadius * samplePositionRadius)
                    {
                        movementSource = ControlMode.Navigation;
                        navAgent.Warp(tmp.position);
                        rb.isKinematic = true;
                        //rb.freezeRotation = true;
                        isLocked = false;
                    }
                }
            }
        }

        //additionalPhysics = null;//consume physics regardless of its use above
        deltaPosForPhys = Vector3.zero;
        deltaRotForPhys = Quaternion.identity;

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

    public IEnumerator<ProgressStatus> TurnToFace(Vector3 lookTarget)
    {
        var result = _TurnToFace(lookTarget);
        result.MoveNext();
        return result;
    }
    private IEnumerator<ProgressStatus> _TurnToFace(Vector3 lookTarget)
    {
        //var desiredLook = Quaternion.LookRotation(lookTarget - transform.position, transform.up).eulerAngles;
        //deltaDegreesV = desiredLook.x;
        //deltaDegreesH = desiredLook.y;
        transform.LookAt(Vector3.ProjectOnPlane(lookTarget, transform.up));
        yield return ProgressStatus.Complete;
        yield break;
    }

    //move this away from IEnumerator
    public IEnumerator<ProgressStatus> MoveToDestination(Vector3 destination, float stoppingDistance = 1)
    {
        var result = _MoveToDestination(destination, stoppingDistance);
        result.MoveNext();
        return result;
    }
    private IEnumerator<ProgressStatus> _MoveToDestination(Vector3 destination, float stoppingDistance)
    {
        if (isLocked || !IsInitialized)
        {
            yield return ProgressStatus.Aborted;
            yield break;
        }

        //clear manual movement
        animMovement = Vector3.zero;
        //clear jump
        jump = false;
        //clear playAnimation
        playAnimationNext = null;
        //clear playAnimation(recoil)
        recoil = RecoilCode.None;

        movementSource = ControlMode.Navigation;
        navAgent.stoppingDistance = stoppingDistance;//dont like setting this here
        navDestination = destination;

        if(Vector3.SqrMagnitude(transform.position - destination) <= Mathf.Pow(stoppingDistance + 0.2f, 2))
        {
            yield return ProgressStatus.Complete;
        }

        while (navAgent.pathPending && !(navAgent.pathStatus == NavMeshPathStatus.PathComplete || navAgent.pathStatus == NavMeshPathStatus.PathPartial))
        {
            if (navDestination != destination)
            {
                yield return ProgressStatus.Aborted;
                yield break;
            }
            else
                yield return ProgressStatus.InProgress;//ProgressStatus.Pending;
        }
        while ( navAgent.isOnOffMeshLink || ( navAgent.isOnNavMesh && navAgent.remainingDistance > 0))
        {
            if ((navDestination != destination) || (!navAgent.isOnNavMesh && !navAgent.isOnOffMeshLink))
            {
                yield return ProgressStatus.Aborted;
                yield break;
            }
            else
                yield return ProgressStatus.InProgress;
        }

        yield return ProgressStatus.Complete;
    }

    public void Jump(Vector3 normalizedJump)
    {
        //todo: nothing here prevents double jumping except animation time length, thats bad design.
        //either this should be an interrupt animation, or it should only jump  if attached to navmesh
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
        anim.ResetTrigger("Abort");
        anim.SetBool("ExitWhenComplete", true);
        remainOnNavMesh = false;
        jump = true;

        normalizedJump = new Vector3(normalizedJump.x * jumpVelocity.x, normalizedJump.y * jumpVelocity.y, normalizedJump.z * jumpVelocity.z);

        //do i want this to scale with creature? thats what transform vector does
        var modifiedVelocity = transform.TransformVector( normalizedJump);
        AddForce(modifiedVelocity, ForceMode.VelocityChange, 0.3f);
    }

    //needs to lock when appropriate
    //unless stayOnNavMesh == true, use physics colliders while animating  

        
    /// <summary>
    /// return value is normalized time. -1 indicates animation is neither playing nor queued
    /// </summary>
    public class PlayToken
    {
        public readonly UnifiedController controller;
        public readonly AnimationClip clip;

        internal PlayToken(UnifiedController controller, AnimationClip clip)
        {
            this.controller = controller;
            this.clip = clip;
        }

        //internal readonly int uniqueID; checking for reference equality, dont need id


        //-1 indicates it isnt in progress
        public float GetProgress()
        {
            var result = -1f;
            if (this == controller.currentToken)
            {
                var stateInfo = controller.anim.GetCurrentAnimatorStateInfo(0);
                var nextInfo = controller.anim.GetNextAnimatorStateInfo(0);

                var isAnimInSlot = controller.overrideController["PlaceHolder_" + controller.playAnimationSlot] == clip;
                var isSlotCurrent = stateInfo.IsName(controller.playAnimationSlot);
                var isSlotNext = nextInfo.IsName(controller.playAnimationSlot);// && anim.IsInTransition(0);
                var isInTransition = controller.anim.IsInTransition(0);

                if (isAnimInSlot && isSlotNext && isInTransition)
                {
                    result = nextInfo.normalizedTime;
                }
                else if (isAnimInSlot && isSlotCurrent)
                {
                    result = stateInfo.normalizedTime;
                }
                else
                {
                    //anim hasnt started yet.
                    result = 0;
                }
            }
            return result;
        }

        public void ExitWhenComplete()
        {
            controller.anim.SetBool("ExitWhenComplete", true);
        }

        public void Abort()
        {
            if( GetProgress() != -1)
            {
                controller.anim.SetTrigger("Abort");
            }
        }
        
        public bool FrameByFrameRemainInState()
        {
            var isInProgress = GetProgress() != -1;
            controller.anim.ResetTrigger("Abort");
            controller.StartCoroutine(ResetRemainInState());
            return isInProgress;
        }

        private IEnumerator ResetRemainInState()
        {
            yield return new WaitForEndOfFrame();
            controller.anim.SetTrigger("Abort");
        }
    }
    
    public PlayToken PlayAnimation(AnimationClip clip, bool remainOnNavMesh = true, bool playMirrored = false, bool exitWhenComplete = true)
    {
        if (isLocked || !IsInitialized || clip == null)
        {
            return null;
        }

        var result = PlayInterruptAnimation(clip, remainOnNavMesh, playMirrored, exitWhenComplete);
        currentToken = result;
        return result;
    }

    public PlayToken PlayInterruptAnimation(AnimationClip clip, bool remainOnNavMesh = true, bool playMirrored = false, bool exitWhenComplete = true)
    {
        //clear manual movement
        animMovement = Vector3.zero;
        //clear navigation
        if(navAgent.isOnNavMesh)
            navAgent.isStopped = true;
        navDestination = transform.position;
        //clear jump
        jump = false;
        //clear playAnimation(recoil)
        recoil = RecoilCode.None;

        this.movementSource = ControlMode.AnimatedRoot;

        anim.ResetTrigger("Abort");
        anim.SetBool("ExitWhenComplete", exitWhenComplete);
        this.remainOnNavMesh = remainOnNavMesh;
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        var nextInfo = anim.GetNextAnimatorStateInfo(0);
        this.playAnimationSlot = "ActionA";
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("ActionA"))
            playAnimationSlot = "ActionB";
        this.playMirrored = playMirrored;
        playAnimationNext = clip;

        isLocked = true;

        var result = new PlayToken(this, clip);
        currentToken = result;
        return result;
    }
    /*
    public class DirectionalAnimationClips
    {
        public readonly AnimationClip up;
        public readonly AnimationClip down;
        public readonly AnimationClip neutral;
        public readonly AnimationClip left;
        public readonly AnimationClip right;

        public DirectionalAnimationClips(AnimationClip up, AnimationClip down, AnimationClip neutral, AnimationClip left, AnimationClip right)
        {
            this.up = up;
            this.down = down;
            this.neutral = neutral;
            this.left = left;
            this.right = right;
        }
    }*/

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        AddForce(force, forceMode, 0);
    }
    public void AddForce(Vector3 force, ForceMode forceMode, float requirePhysicsTimeLength)
    {
        var poise = 8f;
        if(force.sqrMagnitude < poise.Pow(2) && navAgent.isOnNavMesh)
        {
            navAgent.Move(force * Time.deltaTime);
        }
        else
        {
            //need to set mode to physics if using this, else multiple calls will stack
            //clear manual movement
            animMovement = Vector3.zero;
            //clear navigation
            if(navAgent.isOnNavMesh)
                navAgent.isStopped = true;
            navDestination = transform.position;
            //clear playAnimation
            playAnimationNext = null;
            //clear playAnimation(recoil)
            recoil = RecoilCode.None;

            isLocked = true;
            movementSource = ControlMode.Physics;
            additionalPhysics += delegate ()
            {
                this.requirePhysicsTimeLength = Mathf.Max(this.requirePhysicsTimeLength, requirePhysicsTimeLength, Time.deltaTime);
                //this.requirePhysicsTimeLength = requirePhysicsTimeLength;
                rb.AddForce(force, forceMode);
            };
        }
    }
    /*
    public void AddForce(Vector3 force, ForceMode forceMode, bool requirePhysicsUntilRest)
    {
        additionalPhysics += delegate () { rb.AddForce(force, forceMode); };
    }
    */
    public override string ToString()
    {
        return base.ToString() + "\n" +
            "movementSource: " + movementSource + "\n" +
            "transform.position: " + transform.position + "\n" +
            "navAgent.nextPosition: " + navAgent.nextPosition + "\n" +
            "navAgent.pathPending: " + navAgent.pathPending + "\n" +
            "navAgent.pathStatus: " + navAgent.pathStatus + "\n" +
            "navAgent.desiredVelocity: " + navAgent.desiredVelocity + "\n" +
            "navAgent.velocity: " + navAgent.velocity + "\n" +
            "navAgent.destination: " + navAgent.destination + "\n";
    }
}




/*
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

    