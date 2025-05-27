using UnityEngine;

[System.Serializable]
public class FlightPatternComponent : ProjectileComponent
{
    [Header("Wave Settings")]
    [Tooltip("Amplitude of the circular motion, radius of the circle")]
    public float amplitude = 2f;
    [Tooltip("Frequency of the circular motion, in cycles per second")]
    public float frequency = 1f;
    [Tooltip("Primary axis for the circular plane (typically Vector3.up for horizontal circles)")]
    public Vector3 waveAxis = Vector3.up;
    
    private Vector3 originalDirection;
    private Vector3 perpendicular1;
    private Vector3 perpendicular2;

    public override void Initialize(ProjectileRuntimeData data)
    {
        originalDirection = data.initialVelocity.normalized;
        
        // Create first perpendicular vector
        perpendicular1 = Vector3.Cross(originalDirection, waveAxis).normalized;
        if (perpendicular1 == Vector3.zero)
            perpendicular1 = Vector3.Cross(originalDirection, Vector3.up).normalized;
        
        // Create second perpendicular vector (orthogonal to both original direction and first perpendicular)
        perpendicular2 = Vector3.Cross(originalDirection, perpendicular1).normalized;
    }

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        if (!isEnabled) return;

        float time = data.timeAlive * frequency * 2f * Mathf.PI;
        
        // Calculate circular motion using both perpendicular axes
        float cosOffset = Mathf.Cos(time) * amplitude;
        float sinOffset = Mathf.Sin(time) * amplitude;
        
        Vector3 circularPosition = (perpendicular1 * cosOffset) + (perpendicular2 * sinOffset);
        
        // Calculate the position along the original trajectory
        Vector3 basePosition = data.projectile.transform.position;
        Vector3 newPosition = basePosition + circularPosition * Time.deltaTime;
        
        data.projectile.SetPosition(newPosition);
    }
}