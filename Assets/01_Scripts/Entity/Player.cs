using UnityEngine;

public class Player : Entity
{
    private PlayerInputHandler _inputHandler;
    private PlayerController _playerController;

    private HealthComponent _health;
    private StaminaComponent _stamina;
    
    public void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerController = GetComponent<PlayerController>();
        
        _health = GetComponentInChildren<HealthComponent>();
        _stamina = GetComponentInChildren<StaminaComponent>();
    }
    
    public override void Heal(float amount) => _health.Heal(amount);
    public override void Damage(float amount) => _health.TakeDamage(amount);
    public override bool IsAlive() => _health.IsAlive();
    public override void RestoreStamina(float amount) => _stamina.Restore(amount);
    public override void ConsumeStamina(float amount) => _stamina.Consume(amount);
}
