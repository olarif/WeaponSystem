using UnityEngine;

[System.Serializable]
public class RotationData
{
    private PlayerController _controller;
    private Camera _playerCamera;
    private PlayerStatsSO _stats;
    
    [SerializeField] private float xRotation;

    public void Initialize(PlayerController controller)
    {
        _controller = controller;
        _playerCamera = controller.PlayerCamera;
        _stats = controller.Stats;
        
        if (_playerCamera != null)
        {
            xRotation = _playerCamera.transform.localEulerAngles.x;
            if (xRotation > 180f) xRotation -= 360f;
        }
    }

    public void UpdateRotation()
    {
        if (_playerCamera == null) return;
        
        Vector2 rotationInput = _controller.Input.RotationInput;
        float sensitivity = _stats.RotationSpeed;
        
        float verticalRotation = rotationInput.y * sensitivity * Time.deltaTime;
        float horizontalRotation = rotationInput.x * sensitivity * Time.deltaTime;
        
        // Handle vertical rotation (pitch)
        xRotation -= verticalRotation;
        xRotation = Mathf.Clamp(xRotation, _stats.MinLookAngle, _stats.MaxLookAngle);
        _playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        
        // Handle horizontal rotation (yaw)
        _controller.transform.Rotate(Vector3.up * horizontalRotation);
    }
    
}