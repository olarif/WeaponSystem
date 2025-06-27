using System;
using UnityEditor;
using UnityEngine;

public enum AnimationType
{
    Trigger,
    Bool,
    Int,
    Float
}

/// <summary>
/// Triggers an animation on the weapon's Animator based on the specified parameter type and value.
/// </summary>
[Serializable]
public class PlayAnimationAction : IWeaponAction
{
    [Tooltip("Which type of Animator parameter to set")]
    public AnimationType animationType = AnimationType.Trigger;
    [Tooltip("Name of the Animator parameter to modify")]
    public string triggerName;
    [Tooltip("Value used when animationType is Bool")]
    public bool booleanValue = true;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData ab)
    {
        if (ctx.Animator == null || string.IsNullOrEmpty(triggerName))
            return;

        switch (animationType) 
        {
            case AnimationType.Trigger:
                ctx.Animator.SetTrigger(triggerName);
                break;
            case AnimationType.Bool:
                ctx.Animator.SetBool(triggerName, booleanValue);
                break;
            case AnimationType.Int:
                ctx.Animator.SetInteger(triggerName, 1);
                break;
            case AnimationType.Float:
                ctx.Animator.SetFloat(triggerName, 1.0f);
                break;
        }
    }
}