using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class XRSwitchHand : MonoBehaviour
{
    public LaserPointer Laser;
    public Transform LeftHanchor;
    public Transform RightHanchor;
    private OVRInputModule _ovrInputModule;

    private void Start()
    {
        Laser.laserBeamBehavior = LaserPointer.LaserBeamBehavior.On;
        _ovrInputModule = GetComponent<OVRInputModule>();
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {
            _ovrInputModule.rayTransform = LeftHanchor;
            _ovrInputModule.joyPadClickButton = OVRInput.Button.PrimaryIndexTrigger;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            _ovrInputModule.rayTransform = RightHanchor;
            _ovrInputModule.joyPadClickButton = OVRInput.Button.SecondaryIndexTrigger;
        }
    }
}
