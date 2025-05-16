using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GrabOnHit : OnHitComponent
{
    public override void OnHit(CollisionInfo info)
    {
        // This is a placeholder for the actual implementation
        // of the OnHit method. The original code is commented out.
    }
    
    /*
    [Tooltip("The Input Action you hold to grab")]
    public InputActionReference grabReleaseAction;

    [Tooltip("Speed at which object is pulled in")]
    public float grabSpeed = 5f;
    
    public LayerMask grabLayerMask;

    private LineRenderer _lr;
    private GameObject   _target;
    private Coroutine    _pullRoutine;
    private bool         _isGrabbing;
    
    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _lr = ctx.LineRenderer;
        if (_lr != null) _lr.positionCount = 2;

        if (grabReleaseAction?.action != null)
        {
            grabReleaseAction.action.Enable();
            grabReleaseAction.action.canceled += ctx => {
                if (_isGrabbing) Drop();
            };
        }
        else Debug.LogError($"[{name}] grabAction not set!");
    }

    public override void OnHit(CollisionInfo info)
    {
        if (_isGrabbing || info.HitObject == null) return;

        _target     = info.HitObject;
        _isGrabbing = true;
        
        if (_target.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        _pullRoutine = CoroutineRunner.Instance.StartCoroutine(PullRoutine());
    }
    
    public override void Cleanup() => Drop();

    private IEnumerator PullRoutine()
    {
        var firePt = ctx.FirePoint;
        if (_lr != null) _lr.enabled = true;

        while (_isGrabbing && _target != null)
        {
            // move towards the fire point
            _target.transform.position = Vector3.MoveTowards(
                _target.transform.position,
                firePt.position,
                grabSpeed * Time.deltaTime);

            // update line
            if (_lr != null)
            {
                _lr.SetPosition(0, firePt.position);
                _lr.SetPosition(1, _target.transform.position);
            }

            // if close enough, snap onto the firePoint
            if (Vector3.Distance(_target.transform.position, firePt.position) < 0.1f)
            {
                _target.transform.SetParent(firePt, worldPositionStays: false);
                yield break; // end coroutine—but keep the line on until drop
            }

            yield return null;
        }
    }

    private void Drop()
    {
        // stop pulling
        if (_pullRoutine != null)
            CoroutineRunner.Instance.StopCoroutine(_pullRoutine);

        // detach physics and hierarchy
        if (_target != null)
        {
            if (_target.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = false;
            _target.transform.SetParent(null, worldPositionStays: true);
        }

        // hide line
        if (_lr != null)
            _lr.enabled = false;

        // reset state
        _target     = null;
        _isGrabbing = false;
    }
    */
}