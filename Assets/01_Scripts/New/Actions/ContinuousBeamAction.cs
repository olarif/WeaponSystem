using System.Collections;
using UnityEngine;

[System.Serializable]
public class ContinuousBeamAction : WeaponActionData
{
    public GameObject beamPrefab;
    BeamFollower _instance;

    // this now runs on "start holding"
    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        var go = Object.Instantiate(beamPrefab);
        _instance = go.GetComponent<BeamFollower>();
        _instance.origin = ctx.FirePoints[0];
        _instance.length =  b.fireRate > 0 
            ? b.fireRate * 50f 
            : 50f;
    }
    
    public override void OnContinuous(WeaponContext ctx, WeaponDataSO.InputBinding b) { }
    
    public override void OnRelease(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        if (_instance != null)
            Object.Destroy(_instance.gameObject);
    }
}