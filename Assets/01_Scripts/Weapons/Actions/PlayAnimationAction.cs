using System;
using UnityEngine;

public enum AnimationType
{
    Trigger,
    Bool,
    Int,
    Float
}

[Serializable]
public class PlayAnimationAction : IWeaponAction
{
    [Tooltip("You must have a trigger set up in your Animator")]
    public AnimationType animationType = AnimationType.Trigger;
    public string triggerName;
    public bool booleanValue = true;
    //public bool resetTriggerOnEnd = true;

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