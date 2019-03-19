using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlPerformable : IPerformable
{

    Mind performerMind;
    public Mind Performer { get { return performerMind; } }

    Ability punch;
    Transform head;    
    Vector3 cameraOffset = Vector3.zero;//offset from the head
    Vector3 destination;
    bool manualMovement = true;

    public PlayerControlPerformable(Mind performerMind)
    {
        this.performerMind = performerMind;
        punch = new TESTSCRIPT_Punch(performerMind.Body.SubscribeForUpdates, performerMind.Body.SubscribeForAnimationEvents, performerMind.Body.SubscribeForTriggerEvents, performerMind.Body);
        head = performerMind.Body.transform.FindDeepChild("head");

#if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif
        if (Camera.main != null)
            Camera.main.transform.SetParent(performerMind.Body.transform.FindDeepChild("cameraBone"));
    }

    public IEnumerator Perform()
    {
        while (true)
        {
            Camera.main.transform.position = head.position + performerMind.Body.transform.InverseTransformVector(cameraOffset);
            var moveSpeedX = PlayerInput.GetAxis(AxisCode.L_XAxis, 0);
            var moveSpeedZ = PlayerInput.GetAxis(AxisCode.L_YAxis, 0);
            var lookSpeedV = PlayerInput.GetAxis(AxisCode.R_YAxis, 0);
            var lookSpeedH = PlayerInput.GetAxis(AxisCode.R_XAxis, 0);
            var activatePunch = PlayerInput.GetAxis(AxisCode.TriggersR, 0) > 0.75f;
            if (manualMovement)
            {
                performerMind.Body.Move(moveSpeedX, moveSpeedZ);
                performerMind.Body.Look(lookSpeedH, lookSpeedV);
                if (activatePunch)
                    punch.Perform();
            }
            else
            {
                performerMind.Body.MoveToDestination(destination);
            }

            yield return null;
        }
    }
}
