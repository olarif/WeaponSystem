using UnityEngine;

public class Player : Entity
{
    private PlayerInputHandler _inputHandler;
    private PlayerController _playerController;

    private PlayerHealthComponent _health;
    private StaminaComponent _stamina;
    
    public void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerController = GetComponent<PlayerController>();
        _health = GetComponent<PlayerHealthComponent>();
        _stamina = GetComponent<StaminaComponent>();
    }
}
