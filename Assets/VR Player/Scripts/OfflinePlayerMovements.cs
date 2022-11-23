using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class OfflinePlayerMovements : MonoBehaviour
{
    public float PlayerMovementsSpeed = 1f;
    public float PlayerRotationSpeed = 1f;
    public Transform PlayerCamera;
    private CharacterController _controller;
    private Vector2 _movementsInput;
    private Vector2 _rotateInput;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 cameraForward =  Vector3.Scale(PlayerCamera.forward, new Vector3(1f, 0f, 1f)).normalized;
        Vector3 cameraRight = Vector3.Scale(PlayerCamera.right, new Vector3(1f, 0f, 1f)).normalized;
        _movementsInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 movementsDirection = cameraRight * _movementsInput.x + cameraForward * _movementsInput.y;
        _controller.Move(movementsDirection * PlayerMovementsSpeed * Time.deltaTime);
        _rotateInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        transform.Rotate(Vector3.up * _rotateInput.x * PlayerRotationSpeed, Space.World);
    }
}
