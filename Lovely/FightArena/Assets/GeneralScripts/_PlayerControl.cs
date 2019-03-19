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

public class PlayerControl : IDecisionMaker, IPerformable
{
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
        if (performerBody != null && cam != null)
            GameObject.Destroy(cam);
        performerBody = body;
        Performer.OverrideDecisionMaker(this);
        //punch = new PunchCombo(Performer.Body.SubscribeForUpdates, Performer.Body.SubscribeForAnimationEvents, Performer.Body.SubscribeForTriggerEvents, Performer.Body);
        head = Performer.Body.transform.FindDeepChild("head");
        cam = body.GetComponentInChildren<Camera>();
        if(cam == null)
            cam = new GameObject("Camera").AddComponent<Camera>();
        cam.transform.SetParent(Performer.Body.transform.FindDeepChild("cameraBone"));
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;
        SetUpSplitScreen();
    }

    public void GiveUpBody()
    {
        performerBody = null;
    }

    public IPerformable GetDecisions()
    {
        return this;
    }
    
    public IEnumerator Perform()
    {
        while (true)
        {
            if(performerBody != null)
            {
                cam.transform.position = head.position + performerBody.transform.TransformVector(cameraOffset);
                var moveSpeedX = PlayerInput.GetAsAxis(AxisCode.L_XAxis, playerNumber);
                var moveSpeedZ = PlayerInput.GetAsAxis(AxisCode.L_YAxis, playerNumber);
                var lookSpeedV = PlayerInput.GetAsAxis(AxisCode.R_YAxis, playerNumber);
                var lookSpeedH = PlayerInput.GetAsAxis(AxisCode.R_XAxis, playerNumber);
                var activatePunch = PlayerInput.GetAsButtonDown(ButtonCode.B, playerNumber);
                var activateJump = PlayerInput.GetAsButtonDown(ButtonCode.A, playerNumber);
                var empower = PlayerInput.GetAsButtonDown(ButtonCode.Y, playerNumber);

                performerBody.Move(moveSpeedX, moveSpeedZ);
                performerBody.Look(lookSpeedH, lookSpeedV);
                if (activatePunch)
                {
                    if(performerBody.EmpowermentLevel > 0)
                        abilities[CharacterAbilitySlot.DashPunch].CastAbility();
                    else
                        abilities[CharacterAbilitySlot.BasicPunchCombo].CastAbility();


                }
                if (activateJump)
                    performerBody.Jump();
                if (empower)
                    performerBody.Empower();
            }
            yield return null;
        }
    }
}