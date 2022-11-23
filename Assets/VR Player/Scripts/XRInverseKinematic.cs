using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class XRInverseKinematic : MonoBehaviour
{
    public Transform LeftHandAnchor;
    public Transform RightHandAnchor;
    public Transform LookAtAnchor;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        _animator.SetLookAtWeight(0);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandAnchor.position);
        _animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandAnchor.rotation);
        _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandAnchor.position);
        _animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandAnchor.rotation);
        // _animator.SetLookAtPosition(LookAtAnchor.position);
    }
}
