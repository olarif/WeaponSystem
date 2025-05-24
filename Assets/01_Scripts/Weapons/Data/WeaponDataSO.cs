using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ScriptableObject holding configuration for a weapon,
/// including models, UI, default hand, and input bindings.
/// </summary>
[CreateAssetMenu(fileName = "newWeapon", menuName = "Weapon/WeaponSO", order = 0)]
public class WeaponDataSO : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Display name of the weapon.")]
    public string weaponName;

    [Tooltip("Description or lore for the weapon.")]
    [TextArea]
    public string weaponDescription;

    [Header("Model Prefabs")]
    [Tooltip("Model to attach when wielding in the right hand.")]
    public GameObject rightHandModel;

    [Tooltip("Model to attach when wielding in the left hand.")]
    public GameObject leftHandModel;

    [Tooltip("Prefab used for dropping or picking up the weapon in the world.")]
    public GameObject pickupPrefab;

    [Header("Model Transforms")]
    [Tooltip("Local position offset applied to spawned models.")]
    public Vector3 modelPositionOffset;

    [Tooltip("Local rotation offset (Euler angles) applied to spawned models.")]
    public Vector3 modelRotationOffset;

    [Header("Default Settings")]
    [Tooltip("Hand the weapon is wielded in by default.")]
    public Hand defaultHand;

    [Header("Input Bindings")]
    [Tooltip("List of input binding configurations for this weapon.")]
    public List<InputBindingData> inputBindings = new();
}