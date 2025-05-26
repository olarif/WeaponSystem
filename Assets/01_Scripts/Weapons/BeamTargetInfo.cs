using UnityEngine;

public struct BeamTargetInfo
{
    public Vector3 targetPoint;
    public bool hitSomething;
    public float distance;
}

public class BeamInstance
{
    public BeamController controller;
    public GameObject gameObject;

    public void EndBeam(float extraTime = 0f)
    {
        controller?.EndBeam(extraTime);
    }
}