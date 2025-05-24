using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponContext: MonoBehaviour
{
    public PlayerController Player;
    public Transform leftHand, rightHand;
    public Camera PlayerCamera;
    public ChargeUI leftChargeUI;
    public ChargeUI rightChargeUI;
    
    [HideInInspector] public List<Transform> FirePoints = new();
    [HideInInspector] public Animator Animator;
    [HideInInspector] public WeaponController WeaponController;
    [HideInInspector] public WeaponManager    WeaponManager;

    private void Reset()
    {
        if (Player == null) Player = GetComponent<PlayerController>();
        if (PlayerCamera == null) { PlayerCamera = Camera.main; }
    }
    
    public IEnumerable<Transform> GetFirePointsFor(Hand hand)
    {
        if (hand == Hand.Both)
            return FirePoints;

        var parent = (hand == Hand.Left ? leftHand : rightHand);
        return FirePoints.Where(fp => fp.IsChildOf(parent));
    }
}