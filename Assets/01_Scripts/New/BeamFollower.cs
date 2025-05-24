// BeamFollower.cs
using System;
using UnityEngine;

public class BeamFollower : MonoBehaviour
{
    public Transform origin;
    public Transform target;      // optional: if null we use origin.forward + length
    public float     length       = 50f;
    public float     width        = 0.02f;

    public event Action OnBeamStart;
    public event Action OnBeamUpdate;
    public event Action OnBeamEnd;

    LineRenderer _lr;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        if (_lr == null) _lr = gameObject.AddComponent<LineRenderer>();
        _lr.positionCount = 2;
        _lr.startWidth = _lr.endWidth = width;
    }

    void OnEnable()
    {
        OnBeamStart?.Invoke();
    }

    void LateUpdate()
    {
        Vector3 start = origin.position;
        Vector3 end   = target != null
            ? target.position
            : origin.position + origin.forward * length;

        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);

        OnBeamUpdate?.Invoke();
    }

    void OnDisable()
    {
        OnBeamEnd?.Invoke();
    }
}