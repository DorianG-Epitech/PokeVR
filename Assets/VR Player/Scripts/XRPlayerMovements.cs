using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class XRPlayerMovements : NetworkBehaviour
{
    public float PlayerMovementsSpeed = 1f;
    public float PlayerRotationSpeed = 1f;
    public float Gravity = -9.81f;
    public Transform GroundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask GroundMask;
    public Transform PlayerCamera;
    private CharacterController _controller;
    private Vector2 _movementsInput;
    private Vector2 _rotateInput;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        _isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Vector3 cameraForward =  Vector3.Scale(PlayerCamera.forward, new Vector3(1f, 0f, 1f)).normalized;
        Vector3 cameraRight = Vector3.Scale(PlayerCamera.right, new Vector3(1f, 0f, 1f)).normalized;
        _movementsInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 movementsDirection = cameraRight * _movementsInput.x + cameraForward * _movementsInput.y;
        _controller.Move(movementsDirection * PlayerMovementsSpeed * Time.deltaTime);

        _velocity.y += Gravity * Time.deltaTime;

        _controller.Move(_velocity * Time.deltaTime);

        _rotateInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        transform.Rotate(Vector3.up * _rotateInput.x * PlayerRotationSpeed, Space.World);
    }
}
