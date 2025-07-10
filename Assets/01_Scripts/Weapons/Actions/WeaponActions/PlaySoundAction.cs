using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[System.Serializable]
public class PlaySoundAction : IWeaponAction
{
    public EventReference soundEvent;
    public bool isLooping = false;
    
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        FMODUnity.RuntimeManager.PlayOneShot(soundEvent, ctx.transform.position);
    }
}