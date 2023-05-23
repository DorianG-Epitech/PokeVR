using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;

public class PokeballGrabbable : OVRGrabbable
{
    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        base.GrabEnd(linearVelocity, angularVelocity);
        rb.isKinematic = false;
    }
}
