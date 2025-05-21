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
    [HideInInspector] public List<Transform> FirePoints = new List<Transform>();
    [HideInInspector] public Animator Animator;

    private void Reset()
    {
        if (Player == null) Player = GetComponent<PlayerController>();
        if (PlayerCamera == null) { PlayerCamera = Camera.main; }
    }
}