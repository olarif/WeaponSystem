using UnityEngine;

[System.Serializable]
public class FlightPatternComponent : ProjectileComponent
{
    [Header("Wave Settings")]
    public float amplitude = 2f;           // circle radius
    public float frequency = 1f;           // cycles per second
    public Vector3 waveAxis = Vector3.up;  // plane normal

    private Vector3 dir, perp1, perp2;

    public override void Initialize(ProjectileRuntimeData data)
    {
        dir = data.initialVelocity.normalized;
        perp1 = Vector3.Cross(dir, waveAxis);
        if (perp1 == Vector3.zero)
            perp1 = Vector3.Cross(dir, Vector3.up);
        perp1.Normalize();
        perp2 = Vector3.Cross(dir, perp1).normalized;
    }

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        if (!isEnabled) return;

        float t = data.timeAlive * frequency * 2f * Mathf.PI;
        Vector3 offset = perp1 * Mathf.Cos(t) + perp2 * Mathf.Sin(t);
        Vector3 pos = data.projectile.transform.position;

        data.projectile.SetPosition(pos + offset * amplitude * Time.deltaTime);
    }
}
