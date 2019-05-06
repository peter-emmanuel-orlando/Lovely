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
public abstract class UnifiedController : MonoBehaviour, IPhysicsable
{
    [SerializeField]
    public Avatar avatar;

    [SerializeField]
    private AnimatorOverrideController overrideController;


    protected NavMeshAgent navAgent;
    protected Animator anim;
    protected Rigidbody rb;
    private Transform cameraBone;
    public Transform CameraBone { get { return cameraBone; } }
    public NavMeshAgent NavAgent { get { return navAgent; } }
    public bool IsGrounded { get { return  (navAgent.isActiveAndEnabled && navAgent.isOnNavMesh); } }

    private readonly float lookHMaxSpeed = 300;//deg per sec
    private readonly float lookVMaxSpeed = 300;

    private float RunSpeed { get { return overrideController["DefaultMale|RunForward"].averageSpeed.z; } }

    private float WalkSpeed { get { return overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }

    private float WalkBackSpeed { get { return -overrideController["DefaultMale|WalkForward"].averageSpeed.z; } }

    private float WalkSideSpeed { get { return 5f; } }//overrideController["Race:Gender|AnimationName"].averageSpeed.z; } }
    //needs to come from the animations too
    protected Vector3 jumpVelocity = new Vector3(5, 5, 5);

    private Vector3 animMovement;
    private Vector3 animVelo;
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
    { AnimatedNavAgent = 0, Navigation, AnimationRootMotion, Physics }

    private ControlMode movementSource = ControlMode.AnimatedNavAgent;


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
        AwakeEvent?.Invoke(this, new EventArgs());
    }
    protected virtual void Update()
    {
        InnerUpdate();
        UpdateEvent?.Invoke(this, new EventArgs());
    }
    protected virtual void OnDestroy()
    {
        OnDestroyEvent?.Invoke(this, new EventArgs());
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
        //navAgent.hideFlags = HideFlags.NotEditable;
        //navAgent.hideFlags = HideFlags.HideInInspector;
        navAgent.updatePosition = false;
        //navAgent.updateRotation = false;
        navAgent.angularSpeed = lookHMaxSpeed;
        navAgent.speed = RunSpeed;//float.MaxValue;
        navDestination = transform.position;
        //TEMPORARY WORKAROUNT todo
        //navAgent.areaMask &= ~(1 << NavMesh.GetAreaFromName("Jump"));

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
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        rb.useGravity = true;
        rb.mass = 77.7f;
        rb.angularDrag = 50f;
        rb.drag = 1f;
        //rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnAnimatorMove()
    {
        //anim.deltas will be zero if applyrootmotion is set to false and this method doesnt exist
        //this methods existence has the side effect of anim.deltas being calculated, and thus 
        //everything else works as expected
        if(anim.velocity.sqrMagnitude > 0.01) animVelo = anim.velocity;
        //Debug.Log(animVelo);
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
            //multiplying by transform.rotation makes it world space
            //rb.MoveRotation(transform.rotation * (transform.localRotation * Quaternion.Euler(0, deltaDegreesH, 0)));
            //only set lookV if it is between +- maxAngle;
            Quaternion temp = cameraBone.localRotation * Quaternion.Euler(deltaDegreesV, 0, 0);
            if (Quaternion.Angle(Quaternion.identity, temp) < 90f)
                cameraBone.localRotation = temp;
            var lookVerticalDegrees = -Vector3.SignedAngle(transform.forward, cameraBone.forward, transform.right);
            anim.SetFloat("LookVertical", lookVerticalDegrees / 90f);
        }


        if (movementSource == ControlMode.AnimatedNavAgent)
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
        else if (movementSource == ControlMode.AnimationRootMotion)
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
            if(IsGrounded)
            {
                anim.SetFloat("SpeedHorizontal", 0);
                anim.SetFloat("SpeedForward", 0);
            }
        }
    }

    private void SyncNavigation()
    {

        if (movementSource == ControlMode.AnimatedNavAgent)
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
        else if (movementSource == ControlMode.AnimationRootMotion)
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
        //if attached to navmesh
        deltaPosForPhys = anim.deltaPosition;
        deltaRotForPhys = anim.deltaRotation;
        //else enable move via physics
    }

    private void FixedUpdate()
    {
        //certain actions(ie jump, fly, and other vertical movements) 
        //are too complex to be scripted with physics. Therefore follow root motion animation
        //physics (ie falling, ragdolling getting hit by certain things) is to complex to do everything via animation so turn off everything and just physics
        //navigation is a downer to script, its better to use the built in, no matter how troublesome
        Action SynchRbPosToNavViaPhysics = () =>
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            navAgent.Warp(navAgent.nextPosition);
            var desired = ((navAgent.nextPosition - rb.position) / Time.fixedDeltaTime);
            //desired = Vector3.ProjectOnPlane(desired, Vector3.up); //* rb.mass ;
            rb.AddForce(desired, ForceMode.VelocityChange);
            //rb.MovePosition(navAgent.nextPosition);
        };
        Action SynchRbRotToNavViaPhysics = () =>
        {
            rb.MoveRotation(rb.rotation * deltaRotForPhys);
        };

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
        else if (movementSource == ControlMode.AnimatedNavAgent)
        {
            SynchRbPosToNavViaPhysics();
        }
        else if (movementSource == ControlMode.AnimationRootMotion)
        {
            if (remainOnNavMesh)
            {
                //rb.isKinematic = true;
                //rb.MovePosition(navAgent.nextPosition);
                //rb.MoveRotation(rb.rotation * deltaRotForPhys);
                SynchRbPosToNavViaPhysics();
                SynchRbRotToNavViaPhysics();
            }
            else
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.AddForce (deltaPosForPhys / Time.fixedDeltaTime, ForceMode.VelocityChange);
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

            additionalPhysics?.Invoke();
            additionalPhysics = null;

            if (requirePhysicsTimeLength > 0)
            {
                requirePhysicsTimeLength -= Time.fixedDeltaTime;
            }
            else
            {
                if (NavMesh.SamplePosition(lowerCenter, out NavMeshHit tmp, samplePositionRadius, NavMesh.AllAreas))
                {
                    movementSource = ControlMode.Navigation;
                    navAgent.Warp(tmp.position);
                    rb.isKinematic = true;
                    //rb.freezeRotation = true;
                    isLocked = false;
                }
                else if (NavMesh.SamplePosition(headPos, out tmp, samplePositionRadius, NavMesh.AllAreas))
                {
                    var grabPoint = tmp.position;
                    if (NavMesh.FindClosestEdge(grabPoint, out tmp, NavMesh.AllAreas) && Vector3.SqrMagnitude(tmp.position - grabPoint) < samplePositionRadius * samplePositionRadius)
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
        StartCoroutine(Cleanup());
    }

    private IEnumerator Cleanup()
    {
        yield return new WaitForFixedUpdate();
        if(movementSource == ControlMode.Physics || movementSource == ControlMode.AnimatedNavAgent)
        {
            var origRotation = rb.rotation;
            navAgent.Warp(rb.position);
            rb.rotation = origRotation;
        }
        else if (movementSource == ControlMode.Navigation)
            rb.MovePosition(navAgent.nextPosition);
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
        movementSource = ControlMode.AnimatedNavAgent;
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
        //todo, make this operate via rb so it doesnt force an internal recalculation of physics
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

        if (navDestination != destination)
            yield return ProgressStatus.Aborted;
        else if ((destination - transform.position).sqrMagnitude > stoppingDistance.Pow(2))
            yield return ProgressStatus.Failed;
        else
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

        movementSource = ControlMode.AnimationRootMotion;
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

        public void ExitWhenComplete()//change to PauseWhenComplete
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

        this.movementSource = ControlMode.AnimationRootMotion;

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
        AddForce(force, forceMode, requirePhysicsTimeLength = 0.5f);
    }
    public void AddForce(Vector3 force, ForceMode forceMode, float requirePhysicsTimeLength )
    {
        var poise = 8f;
        if(force.sqrMagnitude < poise.Pow(2) && navAgent.isOnNavMesh)
        {
            navAgent.Move(force * Time.deltaTime);
        }
        else
        {
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
            //need to set mode to physics if using this, else multiple calls will stack
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

internal class RigidbodyController
{
    /*
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
            public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

    #if !MOBILE_INPUT
            private bool m_Running;
    #endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;
                if (input.x > 0 || input.x < 0)
                {
                    //strafe
                    CurrentTargetSpeed = StrafeSpeed;
                }
                if (input.y < 0)
                {
                    //backwards
                    CurrentTargetSpeed = BackwardSpeed;
                }
                if (input.y > 0)
                {
                    //forwards
                    //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                    CurrentTargetSpeed = ForwardSpeed;
                }
    #if !MOBILE_INPUT
                if (Input.GetKey(RunKey))
                {
                    CurrentTargetSpeed *= RunMultiplier;
                    m_Running = true;
                }
                else
                {
                    m_Running = false;
                }
    #endif
            }

    #if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
    #endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();


        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;


        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
    #if !MOBILE_INPUT
                return movementSettings.Running;
    #else
                    return false;
    #endif
            }
        }


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init(transform, cam.transform);
        }


        private void Update()
        {
            RotateView();

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump)
            {
                m_Jump = true;
            }
        }


        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {

            Vector2 input = new Vector2
            {
                x = CrossPlatformInputManager.GetAxis("Horizontal"),
                y = CrossPlatformInputManager.GetAxis("Vertical")
            };
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
        */
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

