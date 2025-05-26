using System;
using UnityEngine;

[Serializable]
public class PlayAnimationAction : IWeaponAction
{
    [Tooltip("You must have a trigger set up in your Animator")]
    public string triggerName;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData ab)
    {
        if (ctx.Animator == null || string.IsNullOrEmpty(triggerName))
            return;

        ctx.Animator.SetTrigger(triggerName);
    }
}