using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    public List<ScriptableObject> inputComponents;
    public List<ScriptableObject> executeComponents;
    public List<ScriptableObject> onHitComponents;
}