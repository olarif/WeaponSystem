using UnityEngine;

public class UseConsumableExecute : ExecuteComponent
{
    public ConsumableDataSO consumableData;
    public FireTrigger fireTrigger = FireTrigger.OnPress;

    private Entity _player;
    private int _charges;
    private bool _initialized;

    public override void Initialize(WeaponContext context)
    {
        base.Initialize(context);
        _player = context.Player;
        
        if(!_initialized)
        {
            if (consumableData == null)
                Debug.LogError($"[{name}] missing ConsumableDataSO!");
            _charges      = consumableData?.charges ?? 0;
            _initialized  = true;
        }
    }
    
    public override void OnPress()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnPress)) return;
        if (_charges <= 0)
        {
            _player.GetComponent<WeaponManager>().UnequipWeapon();
            return;
        }
        
        Execute();
    }
    
    void Execute()
    {
        if (_charges <= 0)
        {
            _player.GetComponent<WeaponManager>().UnequipWeapon();
            return;
        }
        
        switch (consumableData.type)
        {
            case ConsumableDataSO.ConsumableType.Health:
                _player.Heal(consumableData.amount);
                break;
            case ConsumableDataSO.ConsumableType.Stamina:
                _player.RestoreStamina(consumableData.amount);
                break;
        }
        
        // Consume one charge
        _charges--;
        Debug.Log($"[{name}] consumed, remaining: {_charges}");

        // If that was the last one, auto‐unequip
        if (_charges <= 0)
            _player.GetComponent<WeaponManager>().UnequipWeapon();
    }
}