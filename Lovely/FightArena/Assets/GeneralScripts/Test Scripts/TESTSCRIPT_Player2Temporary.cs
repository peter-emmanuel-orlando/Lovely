using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class TESTSCRIPT_Player2Temporary : MonoBehaviour, IDecisionMaker, IPerformable
{
    Mind performer;
    public Mind Performer { get { return performer; } }
    Ability punch;
    Transform head;
    Vector3 cameraOffset = Vector3.zero;//offset from the head
    Camera cam;

    private void Start()
    {
        performer = GetComponent<Body>().Mind;
        performer.OverrideDecisionMaker(this);
        //punch = new TESTSCRIPT_Punch(performer.Body.SubscribeForUpdates, performer.Body.SubscribeForAnimationEvents, performer.Body.SubscribeForTriggerEvents, performer.Body);
        head = performer.Body.transform.FindDeepChild("head");
        cam = new GameObject("Camera").AddComponent<Camera>();
        cam.rect = new Rect(0, 0, 1, 0.5f);
        cam.transform.SetParent(performer.Body.transform.FindDeepChild("cameraBone"));
    }

    public IPerformable GetDecisions()
    {
        return this;
    }

    public IEnumerator Perform()
    {
        while (true)
        {
            cam.transform.position = head.position + performer.Body.transform.InverseTransformVector(cameraOffset);
            var moveSpeedX = Convert.ToInt32(Input.GetKey(KeyCode.RightArrow)) - Convert.ToInt32( Input.GetKey(KeyCode.UpArrow));
            var moveSpeedZ = Convert.ToInt32(Input.GetKey(KeyCode.UpArrow)) - Convert.ToInt32(Input.GetKey(KeyCode.DownArrow));
            //var lookSpeedV = PlayerInput.GetAxis(AxisCode.R_YAxis, 0);
            //var lookSpeedH = PlayerInput.GetAxis(AxisCode.R_XAxis, 0);
            var activatePunch = Input.GetKey(KeyCode.RightShift);

            performer.Body.Move(moveSpeedX, moveSpeedZ);
            //performerMind.Body.Look(lookSpeedH, lookSpeedV);
            if (activatePunch)
                punch.CastAbility();

            yield return null;
        }
    }
}
