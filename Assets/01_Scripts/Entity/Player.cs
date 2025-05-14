using UnityEngine;

public class Player : Entity
{
    private PlayerInputHandler _inputHandler;
    private PlayerController _playerController;
    [SerializeField] public PlayerScriptableStats _playerStats;
    
    //private PlayerInventory _playerInventory;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerController = GetComponent<PlayerController>();
        
        // _playerInventory = GetComponent<PlayerInventory>() ?? gameObject.AddComponent<PlayerInventory>();
    }

    //public bool CanEquip(WandEntity wand)
    
}
