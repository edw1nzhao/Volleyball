using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour{
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int) trackedObj.index); } }
    private readonly Valve.VR.EVRButtonId trigger = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private SteamVR_TrackedObject trackedObj;

    private GameObject obj;
    private Rigidbody rb;
    private FixedJoint fJoint;

    private bool throwing;

    private void Start() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        fJoint = GetComponent<FixedJoint>();
    }

    /**
     * Pick up object when trigger is pulled.
     * When released trigger, drop object.
     */
    private void Update() {
        if (Controller != null) {
            var device = SteamVR_Controller.Input((int)trackedObj.index);

            if (Controller.GetPressDown(trigger)) {
                PickUp();
            }

            if (Controller.GetPressUp(trigger)) {
                Drop();
            }
        }
    }

    /**
     * Fixed update since it's physics.
     * Calculate the velocity as the object flies from the controller location.
     * Multiplied by 0.25 to reduce rotational speed when flying.
     * Immediately changed throwing to false to prevent continously adding velocity.
     */
    private void FixedUpdate() {
        if (throwing) {
            Transform origin = trackedObj.origin;
            if (origin == null) {
                origin = trackedObj.transform.parent;
            }

            rb.velocity = origin.TransformVector(Controller.velocity);
            rb.angularVelocity = origin.TransformVector(Controller.angularVelocity * 0.25f);

            rb.maxAngularVelocity = rb.angularVelocity.magnitude;
            throwing = false;
        }
    }

    /**
     * While trigger is held, checks and make sure the object
     * is a "pickupable" object.
     */
    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Pickupable")) {
            obj = other.gameObject;
        }
    }

    /**
     * Release trigger result in no object held!
     */
    private void OnTriggerExit(Collider other) {
        obj = null;
    }

    /**
     * If object is there, then connect fJoint to prevent
     * the object from flying around.
     * 
     */
    private void PickUp() {
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
    private void Drop() {
        if (fJoint.connectedBody != null) {
            rb = fJoint.connectedBody;
            fJoint.connectedBody = null;
            throwing = true;
        }
    }
}