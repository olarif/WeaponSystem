using UnityEngine;

[CreateAssetMenu(menuName = "Projectiles/ChainEffectData")]
public class ChainLightningDataSO : ScriptableObject
{
    public int maxJumps;
    public float jumpDistance;
    public float damagePerJump;
    public float jumpDelay;
}