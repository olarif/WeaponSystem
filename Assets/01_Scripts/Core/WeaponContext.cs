using System.Collections.Generic;
using UnityEngine;

public struct WeaponContext
{
    public Transform FirePoint;
    public Animator Animator;
    public AudioSource AudioSource;
    public LineRenderer LineRenderer;
    public Camera PlayerCamera;
    
    public List<IInputComponent> InputComponents;
    public List<IExecuteComponent> ExecuteComponents;
    public List<IOnHitComponent> OnHitComponents;
}