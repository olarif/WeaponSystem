using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ChargeBeamAction : IWeaponAction
{
    public GameObject beamPrefab;
    public float      duration = 0.5f;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData ab)
    {
        // pick the right FirePoint for this hand
        var fp = ctx.FirePoints
            .First(p => p.IsChildOf(
                b.hand == Hand.Left ? ctx.leftHand : ctx.rightHand));

        // spawn beam
        var go = GameObject.Instantiate(beamPrefab, fp.position, fp.rotation);
        var f  = go.GetComponent<BeamFollower>();
        f.origin = fp;
        f.length = b.holdTime * 50f;

        // auto‐destroy after duration
        ctx.WeaponController.StartCoroutine(AutoDestroy(go, duration));
    }

    IEnumerator AutoDestroy(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        GameObject.Destroy(go);
    }
}
