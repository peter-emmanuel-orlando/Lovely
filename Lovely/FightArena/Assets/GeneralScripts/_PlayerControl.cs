using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class _PlayerControl : MonoBehaviour
{
    [SerializeField]
    int controllerNumber;
    [SerializeField]
    Vector3 cameraOffset;

    PlayerControl playerControl;

    private void Start()
    {
        Update();
    }

    private void Update()
    {
        if (playerControl == null || playerControl.playerNumber != controllerNumber)
        {
            if (playerControl != null) playerControl.GiveUpBody();

            if (controllerNumber == 0)
                playerControl = PlayerControl.GetFirstAvailible();
            else
                playerControl = PlayerControl.GetPlayer(controllerNumber);

            if (playerControl != null)
            {
                controllerNumber = playerControl.playerNumber;
                playerControl.ControlBody(GetComponent<Body>());
            }
        }
        if(playerControl != null)
        {
            playerControl.cameraOffset = cameraOffset;
        }

        if(PlayerControl.GetPlayer(5).IsFree)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (playerControl != null)
            playerControl.GiveUpBody();
    }
}

























public class PlayerControl : Performable, IDecisionMaker
{

    //change to each mindStat in in-game days. negative takes away, positive adds
    public override float DeltaWakefulness { get { return _deltaWakefulness; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public override float DeltaExcitement { get { return _deltaExcitement; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public override float DeltaSpirituality { get { return _deltaSpirituality; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public override float DeltaSocialization { get { return _deltaSocialization; } }//depletes when awake. increases when working or playing together
    public override float DeltaCalories { get { return _deltaCalories; } }//num days calories will last.
    public override float DeltaBlood { get { return _deltaBlood; } }//reaching 0 blood and being passes out
    public override bool IsSleepActivity { get { return _isSleepActivity; } }//is this activity sleeping?

    float _deltaWakefulness = 1;
    float _deltaExcitement = 0;
    float _deltaSpirituality = 0;
    float _deltaSocialization = 0;
    float _deltaCalories = 1;
    float _deltaBlood = 0;
    bool _isSleepActivity = false;
    //*//////////////////////////////////////////////////////////////////////////////////////////////////

    public override ActivityState ActivityType { get { return ActivityState.Work; } }


    private static readonly PlayerControl[] players = new PlayerControl[] { new PlayerControl(1), new PlayerControl(2), new PlayerControl(3), new PlayerControl(4), new PlayerControl(5) };
    public static List<PlayerControl> ConnectedPlayers
    {
        get
        {
            var result = new List<PlayerControl>();
            foreach (var player in players)
            {
                if (!player.IsFree)
                    result.Add(player);
            }
            return result;
        }
    }

    public static PlayerControl GetFirstAvailible()
    {
        PlayerControl result = null;
        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].IsFree && PlayerInput.IsControllerConnected(i+1))
            {
                result = players[i];
                break;
            }
        }
        return result;
    }

    public static PlayerControl GetPlayer(int playerNumber)
    {
        if (playerNumber > 5 || playerNumber < 1)
            throw new InvalidControllerException("player numbers range 1-5, with 5 indicating keyboard");

        return players[playerNumber - 1];
    }

    private static void SetUpSplitScreen()
    {
        var connected = ConnectedPlayers;
        float horizontalScreens = (connected.Count > 1) ? 2 : 1;
        float verticalScreens = (connected.Count > 2) ? 2 : 1;

        var index = 0;
        for (int x = 0; x < horizontalScreens; x++)
        {
            for (int y = 0; y < verticalScreens; y++)
            {
                var current = connected[index];
                if(current != null)
                {
                    current.cam.rect = new Rect(x / horizontalScreens, y / verticalScreens, 1 / horizontalScreens, 1 / verticalScreens);
                    index++;
                }
            }
        }
    }

    public readonly int playerNumber;

    Body performerBody;
    public Mind Performer { get { return performerBody.Mind; } }
    CharacterAbilities abilities { get { return Performer.Body.CharacterAbilities; } }
    Transform head;
    internal Vector3 cameraOffset = Vector3.zero;//offset from the head
    Camera cam;

    public bool IsFree { get { return performerBody == null; } }

    private PlayerControl() { }//dont use

    private PlayerControl(int playerNumber)
    {
        this.playerNumber = playerNumber;
    }

    public void ControlBody(Body body)
    {
        if(body)
        {
            if (performerBody != null && cam != null)
                GameObject.Destroy(cam);
            performerBody = body;
            Performer.OverrideDecisionMaker(this);
            //punch = new PunchCombo(Performer.Body.SubscribeForUpdates, Performer.Body.SubscribeForAnimationEvents, Performer.Body.SubscribeForTriggerEvents, Performer.Body);
            head = Performer.Body.transform.FindDeepChild("head");
            cam = body.GetComponentInChildren<Camera>();
            if (cam == null)
                cam = GameObject.Instantiate<GameObject>(_PrefabPool.GetPrefab("PlayerCamera").GameObject).GetComponent<Camera>();
            if (cam.GetComponent<AudioListener>() == null)
                SingleAudioListner.AttachAudioListner(cam.gameObject);
            cam.allowHDR = true;
            cam.fieldOfView = 60;
            cam.transform.SetParent(Performer.Body.transform.FindDeepChild("cameraBone"));
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
            cam.nearClipPlane = 0.30f;
            cam.allowMSAA = false;

            SetUpSplitScreen();
        }
    }

    public void GiveUpBody()
    {
        performerBody = null;
    }

    public IPerformable GetDecisions()
    {
        return this;
    }

    private GameObject lockOnTarget = null;
    private void VisualLockOn()
    {
        if(lockOnTarget == null)
        {
            RaycastHit rInfo;
            var lookRay = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.SphereCast(lookRay, 3f, out rInfo, 40f, ~(LayerMask.NameToLayer("HitBox") | LayerMask.NameToLayer("HurtBox")), QueryTriggerInteraction.Collide))
            {
                lockOnTarget = rInfo.collider.gameObject;
            }
        }
        else
        {
            lockOnTarget = null;
        }
    }

    public override IEnumerator Perform()
    {
        while (true)
        {
            if (performerBody != null)
            {
                cam.transform.position = head.position + performerBody.transform.TransformVector(cameraOffset);
                var empower = PlayerInput.GetAsButtonDown(ButtonCode.Y, playerNumber);
                var moveSpeedX = PlayerInput.GetAsAxis(AxisCode.L_XAxis, playerNumber);
                var moveSpeedZ = PlayerInput.GetAsAxis(AxisCode.L_YAxis, playerNumber);
                var lookSpeedV = PlayerInput.GetAsAxis(AxisCode.R_YAxis, playerNumber);
                var lookSpeedH = PlayerInput.GetAsAxis(AxisCode.R_XAxis, playerNumber);
                var activatePunch = PlayerInput.GetAsButtonDown(ButtonCode.B, playerNumber);
                var activateRanged = PlayerInput.GetAsButtonDown(ButtonCode.X, playerNumber);
                var activateJump = PlayerInput.GetAsButtonDown(ButtonCode.A, playerNumber);
                var block = PlayerInput.GetAsButton(AxisCode.TriggersL, playerNumber) || PlayerInput.GetAsButton(AxisCode.TriggersR, playerNumber);
                var lockOntoTarget = PlayerInput.GetAsButtonDown(ButtonCode.RS, playerNumber);

                if (lockOntoTarget)
                    VisualLockOn();
                if (activatePunch)
                {
                    if(performerBody.EmpowermentLevel > 0)
                        abilities[CharacterAbilitySlot.DashPunch].CastAbility();
                    else
                        abilities[CharacterAbilitySlot.BasicPunchCombo].CastAbility();
                }
                if(activateRanged)
                {
                    if (performerBody.EmpowermentLevel > 0)
                        abilities[CharacterAbilitySlot.RangedPower].CastAbility();
                    else
                        abilities[CharacterAbilitySlot.ThrowItem].CastAbility();
                }
                if (block)
                {
                    if(PlayerInput.GetAsButton(AxisCode.L_XAxis, playerNumber) || PlayerInput.GetAsButton(AxisCode.L_YAxis, playerNumber))
                    {
                        var dodge = abilities[CharacterAbilitySlot.Dodge] as Dodge;
                        if(dodge != null)//cant assume ability will really be of a specific type
                            dodge.CastAbility(new Vector3(moveSpeedX, 0, moveSpeedZ));
                        else
                            abilities[CharacterAbilitySlot.Block].CastAbility();
                    }
                    else
                        abilities[CharacterAbilitySlot.Block].CastAbility();
                }
                if (activateJump)
                    performerBody.Jump(new Vector3(moveSpeedX, 1, moveSpeedZ));
                if (empower)
                    performerBody.Empower();


                if (lockOnTarget)
                    performerBody.TurnToFace(lockOnTarget.transform.position);
                performerBody.Move(moveSpeedX, moveSpeedZ);
                performerBody.Look(lookSpeedH, lookSpeedV);

            }
            yield return null;
        }
    }
}