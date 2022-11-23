using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFootIK : MonoBehaviour
{
    public Vector3 FootOffset;
    [Range(0, 1)]
    public float RightFootPosWeight = 1;
    [Range(0, 1)]
    public float RightFootRotWeight = 1;
    [Range(0, 1)]
    public float LeftFootPosWeight = 1;
    [Range(0, 1)]
    public float LeftFootRotWeight = 1;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        RaycastHit hit;
        Vector3 rightFootPos = _animator.GetIKPosition(AvatarIKGoal.RightFoot);
        Vector3 leftFootPos = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);

        if (Physics.Raycast(rightFootPos + Vector3.up, Vector3.down, out hit))
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, RightFootPosWeight);
            _animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + FootOffset);

            Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, RightFootRotWeight);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, footRotation);
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
        }


        if (Physics.Raycast(leftFootPos + Vector3.up, Vector3.down, out hit))
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, LeftFootPosWeight);
            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point  + FootOffset);

            Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, LeftFootRotWeight);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, footRotation);
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
        }
    }
}
