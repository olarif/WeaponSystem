using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Holds shared references and utilities for weapon systems on the player.
/// Manages fire points, hand transforms, UI, and related managers.
/// </summary>
[DisallowMultipleComponent]
public class WeaponContext : MonoBehaviour
{
    [Header("Player & Camera")]
    [Tooltip("Reference to the player controller.")]
    public PlayerController Player;

    [Tooltip("Transform of the left hand mount.")]
    public Transform leftHand;

    [Tooltip("Transform of the right hand mount.")]
    public Transform rightHand;

    [Tooltip("Main camera used for aiming and raycasts.")]
    public Camera PlayerCamera;

    [Header("UI References")]
    [Tooltip("UI element for left-hand charge indicator.")]
    public ChargeUI leftChargeUI;

    [Tooltip("UI element for right-hand charge indicator.")]
    public ChargeUI rightChargeUI;

    [Header("Dynamic Data")]
    [HideInInspector]
    [Tooltip("All valid fire points attached to the current weapon model(s).")]
    public List<Transform> FirePoints = new List<Transform>();
    
    //[Tooltip("Cached Animator from the weapon or player.")]
    public Animator Animator;

    [HideInInspector]
    [Tooltip("Active WeaponController reference.")]
    public WeaponController WeaponController;

    [HideInInspector]
    [Tooltip("Managing component for switching and tracking weapons.")]
    public WeaponManager WeaponManager;

    /// <summary>
    /// Auto-fill common references when component is reset in the editor.
    /// </summary>
    private void Reset()
    {
        if (Player == null)
            Player = GetComponent<PlayerController>();

        if (PlayerCamera == null)
            PlayerCamera = Camera.main;
    }
    
    public IEnumerable<Transform> GetFirePointsFor(Hand hand)
    {
        // Return all points if both hands
        if (hand == Hand.Both)
            return FirePoints.Where(fp => fp != null);

        // Select only those under the chosen hand transform
        var parent = hand == Hand.Left ? leftHand : rightHand;
        return FirePoints
            .Where(fp => fp != null && fp.IsChildOf(parent));
    }
}
