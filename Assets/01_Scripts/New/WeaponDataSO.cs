using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "newWeapon", menuName = "Weapon/WeaponSO", order = 0)]
public class WeaponDataSO : ScriptableObject
{
    public string weaponName;
    public string weaponDescription;
    public GameObject rightHandModel;
    public GameObject leftHandModel;
    public GameObject pickupPrefab;
    public Vector3 modelPositionOffset;
    public Vector3 modelRotationOffset;
    public Hand defaultHand;
    
    public List<InputBindingData> inputBindings = new();
}