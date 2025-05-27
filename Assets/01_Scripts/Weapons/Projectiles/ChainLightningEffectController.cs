using UnityEngine;

[System.Serializable]
public class ChainLightningEffectController : MonoBehaviour
{
    [Header("Travel Settings")]
    public float travelSpeed = 20f;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private GameObject targetObject;
    private float damage;
    private DamageType damageType;
    private float journeyLength;
    private float journeyTime = 0f;
    private bool isTraveling = false;

    public void StartTravel(Vector3 from, Vector3 to, GameObject target, float dmg, DamageType dmgType)
    {
        startPosition = from;
        targetPosition = to;
        targetObject = target;
        damage = dmg;
        damageType = dmgType;
        
        transform.position = startPosition;
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        journeyTime = 0f;
        isTraveling = true;
        
        // Point towards target
        if (journeyLength > 0)
        {
            transform.LookAt(targetPosition);
        }
    }

    private void Update()
    {
        if (!isTraveling) return;

        // Move towards target
        journeyTime += Time.deltaTime;
        float distanceCovered = journeyTime * travelSpeed;
        float fractionOfJourney = distanceCovered / journeyLength;

        if (fractionOfJourney >= 1f)
        {
            // Reached target
            transform.position = targetPosition;
            OnReachTarget();
            isTraveling = false;
        }
        else
        {
            // Continue traveling
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
        }
    }

    private void OnReachTarget()
    {
        // Deal damage when we reach the target
        if (targetObject != null && targetObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, damageType);
        }

        // Note: Target is already added to hit list in the main chain sequence
        // to prevent the same target from being selected multiple times
    }
}