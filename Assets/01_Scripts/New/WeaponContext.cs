using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponContext: MonoBehaviour
{
    public PlayerController Player;
    public Transform leftHand;
    public Transform rightHand;
    public Camera PlayerCamera;
    
    public ChargeUI leftChargeUI;
    public ChargeUI rightChargeUI;
    
    [HideInInspector] public List<Transform> FirePoints = new List<Transform>();
    [HideInInspector] public Animator Animator;
    
    [HideInInspector] public WeaponController WeaponController;
    [HideInInspector] public WeaponManager    WeaponManager;

    private void Reset()
    {
        if (Player == null) Player = GetComponent<PlayerController>();
        if (PlayerCamera == null) { PlayerCamera = Camera.main; }
    }
}