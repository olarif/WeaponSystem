using System.Collections;
using UnityEngine;

public class GrabOnHit : OnHitComponent
{
    public GrabDataSO grabData;
    
    private LineRenderer _lr;

    private bool _isGrabbing;

    public override void Initialize(WeaponContext context)
    {
        WeaponContext = context;
        _lr = context.LineRenderer;
    }

    public override void OnHit(CollisionInfo info)
    {
        if(_isGrabbing || info.HitObject == null) return;
        
        GameObject grabbedObject = info.HitObject;
        _isGrabbing = true;

        if (grabbedObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        CoroutineRunner.Instance.StartCouroutine(PullObject(grabbedObject));
    }

    private IEnumerator PullObject(GameObject target)
    {
        Transform firePoint = WeaponContext.FirePoint;

        while (Vector3.Distance(target.transform.position, firePoint.position) > 0.1f && _isGrabbing)
        {
            target.transform.position = Vector3.MoveTowards(
                target.transform.position,
                firePoint.position,
                grabData.grabSpeed * Time.deltaTime
            );

            UpdateLine(firePoint.position, target.transform.position);

            yield return null;
        }

        // Attach to hand after pulling
        target.transform.SetParent(firePoint);
        target.transform.position = firePoint.position;
        target.transform.rotation = firePoint.rotation;
        
        UpdateLine(firePoint.position, target.transform.position);
    }
    
    private void UpdateLine(Vector3 start, Vector3 end)
    {
        if(!_lr.enabled)
            _lr.enabled = true;
        
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);
    }
}