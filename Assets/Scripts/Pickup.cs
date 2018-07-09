using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private readonly Valve.VR.EVRButtonId trigger = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private SteamVR_TrackedObject trackedObj;

    private GameObject obj;
    private Rigidbody rb;
    private FixedJoint fJoint;

    private bool throwing;
    private bool ballHit;
    private bool triggerHeld;

    void Start() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        fJoint = GetComponent<FixedJoint>();
    }

    /**
     * Pick up object when trigger is pulled.
     * When released trigger, drop object.
     */
    void Update() {
        if (Controller != null) {
            var device = SteamVR_Controller.Input((int)trackedObj.index);

            if (Controller.GetPressDown(trigger)) {
                triggerHeld = true;
                PickUp();
            }

            if (Controller.GetPressUp(trigger)) {
                triggerHeld = false;
                Drop();
            }
        }
    }

    /**
     * Fixed update since it's physics.
     * Calculate the velocity as the object flies from the controller location.
     * Immediately changed throwing to false to prevent continously adding velocity.
     */
    void FixedUpdate() {
        if (throwing) {
            Transform origin = trackedObj.origin;
            if (origin == null) {
                origin = trackedObj.transform.parent;
            }

            rb.velocity = origin.TransformVector(Controller.velocity);
            rb.angularVelocity = origin.TransformVector(Controller.angularVelocity);

            rb.maxAngularVelocity = rb.angularVelocity.magnitude;
            throwing = false;
        }

        // Changed into else if. If not, then change to only if
        // Check if trigger && ballhit, then change physics of 
        // Hitting the ball to have it stick
        // Look into hitting a ball and only sending it downwards?
        if (triggerHeld && ballHit) {
            PickUp();
        } else if (ballHit) {
            Transform origin = trackedObj.origin;

            if (origin == null) {
                origin = trackedObj.transform.parent;
            }

            Debug.Log("origin", origin);

            rb.velocity = origin.TransformVector(Controller.velocity * 1.5f);
            rb.angularVelocity = origin.TransformVector(Controller.angularVelocity);

            rb.maxAngularVelocity = rb.angularVelocity.magnitude;
        }
    }


    void HitBall() {
        Transform origin = trackedObj.origin;
        if (origin == null) {
            origin = trackedObj.transform.parent;
        }

        rb.velocity = origin.TransformVector(Controller.velocity);
        rb.angularVelocity = origin.TransformVector(Controller.angularVelocity * 0.5f);

        rb.maxAngularVelocity = rb.angularVelocity.magnitude;
        throwing = false;
    }

    /**
     * While trigger is held, checks and make sure the object
     * is a "pickupable" object.
     */
    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Pickupable") && triggerHeld) {
            obj = other.gameObject;
        }
    }

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Pickupable") && !triggerHeld) {
            ballHit = true;
        }
    }

    /**
     * Release trigger result in no object held!
     */
    void OnTriggerExit(Collider other) {
        Debug.Log("False");

        obj = null;
        ballHit = false;
    }

    /**
     * If object is there, then connect fJoint to prevent
     * the object from flying around.
     * 
     */
    void PickUp() {
        if (obj != null) {
            fJoint.connectedBody = obj.GetComponent<Rigidbody>();
            throwing = false;
            rb = null;
        } else {
            fJoint.connectedBody = null;
        }
    }

    /**
     * Drop object.
     * Gets rid of fjoint
     * Intiates throwing!
     */
    void Drop() {
        if (fJoint.connectedBody != null) {
            rb = fJoint.connectedBody;
            fJoint.connectedBody = null;
            throwing = true;
        }
    }
}