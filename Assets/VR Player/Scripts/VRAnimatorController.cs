using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAnimatorController : MonoBehaviour
{
    public float SpeedTreshold = 0.1f;
    [Range(0, 1)]
    public float Smoothing = 1;
    private Animator _animator;
    private VRRig _vrRig;
    private Vector3 _previousPos;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vrRig = GetComponent<VRRig>();
        _previousPos = _vrRig.HeadMap.VrTarget.position;
    }

    private void Update()
    {
        Vector3 headVelocity = (_vrRig.HeadMap.VrTarget.position - _previousPos) / Time.deltaTime;
        headVelocity.y = 0;

        Vector3 headLocalVelocity = transform.InverseTransformDirection(headVelocity);
        _previousPos = _vrRig.HeadMap.VrTarget.position;

        float previousDirectionX = _animator.GetFloat("DirectionX");
        float previousDirectionY = _animator.GetFloat("DirectionY");

        _animator.SetBool("IsMoving", headLocalVelocity.magnitude > SpeedTreshold);
        _animator.SetFloat("DirectionX", Mathf.Lerp(previousDirectionX, Mathf.Clamp(headLocalVelocity.x, -1, 1), Smoothing));
        _animator.SetFloat("DirectionY", Mathf.Lerp(previousDirectionY, Mathf.Clamp(headLocalVelocity.z, -1, 1), Smoothing));
    }
}
