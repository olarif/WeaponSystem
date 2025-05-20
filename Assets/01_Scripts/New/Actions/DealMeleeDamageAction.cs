using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DealDamageAction : WeaponActionData
{
    public DamageType damageType;
    public float damage = 5f;
    public float range = 2.5f;
    
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Debug.Log("DealDamageAction executed!");
        
        //physics overlap
        Collider[] hitColliders = Physics.OverlapSphere(ctx.transform.position, range);
        foreach (var hitCollider in hitColliders)
        {
            // Assuming the target is damageable
            var target = hitCollider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage, damageType);
                Debug.Log("Dealt " + damage + " damage to " + hitCollider.name);
            }
            
        }
    }
}