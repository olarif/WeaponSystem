using System.Collections.Generic;
using UnityEngine;

public class WeaponContext
{
    public Transform FirePoint;
    public Animator Animator;
    public AudioSource AudioSource;
    public LineRenderer LineRenderer;
    public Camera PlayerCamera;
    public List<IOnHitComponent> OnHitComponents = new List<IOnHitComponent>();
}