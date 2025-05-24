using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class PressBeamAction : WeaponActionData
{
    public GameObject beamPrefab;
    public float      duration = 0.3f;

    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        // spawn follower
        var go = Object.Instantiate(beamPrefab, ctx.FirePoints[0].position, ctx.FirePoints[0].rotation);
        var f  = go.GetComponent<BeamFollower>();
        f.origin = ctx.FirePoints[0];
        f.length = b.fireRate > 0 ? b.fireRate * 50f : 50f;  // example

        // auto-destroy after duration
        CoroutineRunner.Instance.StartCoroutine(AutoDestroy(go, duration));
    }

    IEnumerator AutoDestroy(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        Object.Destroy(go);
    }
}