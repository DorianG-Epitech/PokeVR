using UnityEngine;

public class OrientateToCamera : MonoBehaviour
{
    public Transform _mainCamera;

    void Update()
    {
        transform.LookAt(2 * transform.position - _mainCamera.position);
    }
}
