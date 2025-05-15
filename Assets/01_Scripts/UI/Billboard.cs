using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_camera != null)
        {
            var rotation = _camera.transform.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);
        }
    }
}
