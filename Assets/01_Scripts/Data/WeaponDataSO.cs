using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    public List<WeaponComponent> Components;

    public string weaponName;
    public string weaponDescription;
    public GameObject weaponPrefab;
    
    [Header("Component Lists")]
    public List<ScriptableObject> inputComponents;
    public List<ScriptableObject> executeComponents;
    public List<ScriptableObject> onHitComponents;
}