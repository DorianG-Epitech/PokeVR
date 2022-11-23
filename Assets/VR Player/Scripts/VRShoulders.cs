using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(VRRig))]
public class VRShoulders : MonoBehaviour
{
    public float ShoulderRotateSpeed = 1f;
    public Transform LeftHandTarget;
    public Transform LeftHandBone;
    public Transform RightHandTarget;
    public Transform RightHandBone;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        float currentLeftShoulderWeight = _animator.GetFloat("LeftShoulderRotation");
        if (transform.InverseTransformDirection(LeftHandTarget.position - LeftHandBone.position).z > 0.05f)
        {
            _animator.SetFloat("LeftShoulderRotation", Mathf.Lerp(currentLeftShoulderWeight, 1, ShoulderRotateSpeed * Time.deltaTime));
        }
        else if (currentLeftShoulderWeight > 0.01f)
        {
            float newLeftShoulderWeight = Mathf.Lerp(currentLeftShoulderWeight, 0, ShoulderRotateSpeed * Time.deltaTime);

            if (newLeftShoulderWeight < 0.01f)
                newLeftShoulderWeight = 0;
            _animator.SetFloat("LeftShoulderRotation", Mathf.Lerp(currentLeftShoulderWeight, 0, ShoulderRotateSpeed * Time.deltaTime));
        }

        float currentRightShoulderWeight = _animator.GetFloat("RightShoulderRotation");
        if (transform.InverseTransformDirection(RightHandTarget.position - RightHandBone.position).z > 0.05f)
        {
            _animator.SetFloat("RightShoulderRotation", Mathf.Lerp(currentRightShoulderWeight, 1, ShoulderRotateSpeed * Time.deltaTime));
        }
        else if (currentRightShoulderWeight > 0.01f)
        {
            float newRightShoulderWeight = Mathf.Lerp(currentRightShoulderWeight, 0, ShoulderRotateSpeed * Time.deltaTime);

            if (newRightShoulderWeight < 0.01f)
                newRightShoulderWeight = 0;
            _animator.SetFloat("RightShoulderRotation", Mathf.Lerp(currentRightShoulderWeight, 0, ShoulderRotateSpeed * Time.deltaTime));
        }
    }
}
