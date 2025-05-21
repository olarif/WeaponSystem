using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ContinuousRayAction : WeaponActionData
{
    public enum BeamMode
    {
        WhileHold,
        Timed,
        Charge
    }

    public float maxDistance = 100f;
    public DamageType damageType = DamageType.Physical;
    public float damage = 10f;
    
    [Header("Beam Settings")]
    public BeamMode beamMode = BeamMode.WhileHold;
    public GameObject linePrefab;
    public float beamDuration = 1f;
    public float chargeTime = 1f;
    public float pulseTime = 0.1f;
    public float width = 0.02f;
    
    Dictionary<WeaponContext, float>  _holdTimers   = new();
    Dictionary<WeaponContext, Coroutine> _beamRoutines = new();
    
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Debug.Log("ContinuousRayAction executed");
        
        Vector3 origin = ctx.FirePoints.Count > 0 ? ctx.FirePoints[0].position : ctx.rightHand.position;
        Vector3 dir    = (ctx.FirePoints.Count > 0 ? ctx.FirePoints[0].forward : ctx.rightHand.forward);

        if (Physics.Raycast(origin, dir, out var hit, maxDistance))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(damage, damageType);
        }

        // 2) Handle the beam drawing
        switch (beamMode)
        {
            case BeamMode.WhileHold:
                DrawBeam(origin, hit.collider != null ? hit.point : origin + dir * maxDistance, beamDuration);
                break;

            case BeamMode.Timed:
                // start one timed beam and don’t restart if it’s already running
                if (!_beamRoutines.ContainsKey(ctx))
                {
                    _beamRoutines[ctx] = CoroutineRunner.Instance.StartRoutine(
                        BeamCoroutine(ctx, origin, hit.collider != null ? hit.point : origin + dir * maxDistance, beamDuration)
                    );
                }
                break;

            case BeamMode.Charge:
                // accumulate hold time
                if (!_holdTimers.ContainsKey(ctx))
                    _holdTimers[ctx] = 0f;
                _holdTimers[ctx] += binding.fireRate;

                // once we hit chargeTime, pulse once, reset timer
                if (_holdTimers[ctx] >= chargeTime)
                {
                    CoroutineRunner.Instance.StartRoutine(
                        BeamCoroutine(ctx, origin, hit.collider != null ? hit.point : origin + dir * maxDistance, pulseTime)
                    );
                    _holdTimers[ctx] = 0f;
                }
                break;
        }
    }

    private IEnumerator BeamCoroutine(WeaponContext ctx, Vector3 start, Vector3 end, float duration)
    {
        // spawn the LineRenderer
        GameObject go = linePrefab != null
            ? Object.Instantiate(linePrefab)
            : new GameObject("Beam");
        var lr = go.GetComponent<LineRenderer>() ?? go.AddComponent<LineRenderer>();

        // configure fallback
        if (linePrefab == null)
        {
            lr.startWidth = lr.endWidth = width;
            lr.positionCount = 2;
        }

        // draw once
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // let it live
        yield return new WaitForSeconds(duration);

        // clean up
        Object.Destroy(go);
        _beamRoutines.Remove(ctx);
    }

    private void DrawBeam(Vector3 start, Vector3 end, float duration)
    {
        // instant‐destroy version for WhileHeld
        var go = linePrefab != null
            ? Object.Instantiate(linePrefab)
            : new GameObject("Beam");
        var lr = go.GetComponent<LineRenderer>() ?? go.AddComponent<LineRenderer>();

        if (linePrefab == null)
        {
            lr.startWidth = lr.endWidth = width;
            lr.positionCount = 2;
        }

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Object.Destroy(go, duration);
    }
}