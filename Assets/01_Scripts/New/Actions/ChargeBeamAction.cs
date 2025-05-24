using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChargeBeamAction : WeaponActionData
{
    public GameObject beamPrefab;
    public float      duration = 0.5f;

    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        // runner only calls this if holdTime was reached
        var go = Object.Instantiate(beamPrefab);
        var f  = go.GetComponent<BeamFollower>();
        f.origin = ctx.FirePoints[0];
        f.length = b.holdTime * 50f;

        CoroutineRunner.Instance.StartCoroutine(AutoDestroy(go, duration));
    }

    IEnumerator AutoDestroy(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        Object.Destroy(go);
    }
}
