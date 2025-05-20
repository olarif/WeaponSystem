using System;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponContext: MonoBehaviour
{
    public PlayerController Player;
    public Transform leftHand;
    public Transform rightHand;
    public Animator Animator;
    public Camera PlayerCamera;

    private void Reset()
    {
        //auto assign references if left blank
        
        if (Player == null) Player = GetComponent<PlayerController>();
        if (Animator == null) Animator = GetComponent<Animator>();
        if (PlayerCamera == null) { PlayerCamera = Camera.main; }
    }
}