using UnityEngine;

public class Player : Entity
{
    private PlayerInputHandler _inputHandler;
    private PlayerController _playerController;
    
    //private PlayerInventory _playerInventory;

    public void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerController = GetComponent<PlayerController>();
        
        // _playerInventory = GetComponent<PlayerInventory>() ?? gameObject.AddComponent<PlayerInventory>();
    }
}
