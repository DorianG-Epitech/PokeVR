using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform VrTarget;
    public Transform RigTarget;
    public Vector3 TrackingPositionOffset;
    public Vector3 TrackingRotationOffset;

    public void Map()
    {
        RigTarget.position = VrTarget.TransformPoint(TrackingPositionOffset);
        RigTarget.rotation = VrTarget.rotation * Quaternion.Euler(TrackingRotationOffset);
    }
}

public class VRRig : MonoBehaviour
{
    public VRMap HeadMap;
    public VRMap LeftHandMap;
    public VRMap RightHandMap;
    public Transform HeadConstraint;
    public float TurnSmoothness = 1f;
    public Vector3 _headBodyOffset;

    private void Start()
    {
        _headBodyOffset = transform.position - HeadConstraint.position;
    }

    private void FixedUpdate()
    {
        transform.position = HeadConstraint.position + _headBodyOffset;
        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(HeadConstraint.forward, Vector3.up).normalized, Time.deltaTime * TurnSmoothness);

        HeadMap.Map();
        LeftHandMap.Map();
        RightHandMap.Map();
    }
}
