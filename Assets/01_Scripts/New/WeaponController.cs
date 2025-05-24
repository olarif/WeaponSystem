using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    WeaponDataSO        _data;
    WeaponContext       _ctx;
    
    [HideInInspector] public List<GameObject> _models = new();
    
    public void Initialize(WeaponDataSO data, WeaponContext ctx)
    {
        _data = data;
        _ctx  = ctx;
        _ctx.WeaponController = this;

        AttachModel();
        CreateBindingRunners();
    }
    
    void CreateBindingRunners()
    {
        // For each binding, spin up one runner component
        foreach (var binding in _data.bindings)
        {
            var runner = gameObject.AddComponent<WeaponBindingRunner>();
            runner.Setup(binding, _ctx);
        }
    }

    void AttachModel()
    {
        if (_data == null) return;
        _ctx.FirePoints.Clear();
        _models.Clear();

        // helper to spawn one side
        void SpawnSide(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null) return;
            var mdl = Instantiate(prefab, parent);
            mdl.transform.localPosition = _data.modelPositionOffset;
            mdl.transform.localRotation = Quaternion.Euler(_data.modelRotationOffset);

            _models.Add(mdl);

            // register its FirePoint
            var fp = mdl.transform.Find("FirePoint");
            if (fp != null)
                _ctx.FirePoints.Add(fp);
            else
                Debug.LogWarning($"[{name}] no ‘FirePoint’ child on {mdl.name}", this);
        }

        // decide which to spawn
        switch (_data.defaultHand)
        {
            case WeaponDataSO.Hand.Right:
                SpawnSide(_data.rightHandModel, _ctx.rightHand);
                break;
            case WeaponDataSO.Hand.Left:
                SpawnSide(_data.leftHandModel, _ctx.leftHand);
                break;
            case WeaponDataSO.Hand.Both:
                SpawnSide(_data.rightHandModel, _ctx.rightHand);
                SpawnSide(_data.leftHandModel,  _ctx.leftHand);
                break;
        }
    }
}
