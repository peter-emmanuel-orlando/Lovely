using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT_PlayerUnifiedController : Being
{
    Ability punch;
    Transform head;

    [SerializeField]
    Vector3 cameraOffset = Vector3.zero;//offset from the head
    [SerializeField]
    Vector3 destination;
    [SerializeField]
    bool manualMovement;



    protected override void Awake()
    {
        base.Awake();
        destination = transform.position;

#if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif
        if(Camera.main != null)
            Camera.main.transform.SetParent(transform);
        head = transform.FindDeepChild("head");

        punch = new TESTSCRIPT_Punch(SubscribeForUpdates, SubscribeForAnimationEvents, SubscribeForTriggerEvents, this);
    }

    protected override void Update()
    {
        base.Update();

        Camera.main.transform.position = head.position + transform.InverseTransformVector(cameraOffset);
        var moveSpeedX = PlayerInput.GetAxis(AxisCode.L_XAxis, 0);
        var moveSpeedZ = PlayerInput.GetAxis(AxisCode.L_YAxis, 0);
        var lookSpeedV = PlayerInput.GetAxis(AxisCode.R_YAxis, 0);
        var lookSpeedH = PlayerInput.GetAxis(AxisCode.R_XAxis, 0);
        var activatePunch = PlayerInput.GetAxis(AxisCode.TriggersR, 0) > 0.5f;
        if(manualMovement)
        {
            Move(moveSpeedX, moveSpeedZ);
            Look(lookSpeedH, lookSpeedV);
            if (activatePunch)
                punch.Perform();
        }
        else
        {
            MoveToDestination(destination);
        }
    }    
}
