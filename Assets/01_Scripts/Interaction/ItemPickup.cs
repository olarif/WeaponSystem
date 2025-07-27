using UnityEngine;

public class ItemPickup : BasePickup
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    
    public override string GetPromptText()
    {
        return itemData != null ? $"E to pickup {itemData.itemName} x{quantity}" : "E to pickup item";
    }
    
    public override bool CanPickup(PlayerInventory inventory)
    {
        return itemData != null && inventory.CanAddItem(itemData, quantity);
    }
    
    public override void OnPickup(PlayerInventory inventory)
    {
        if (inventory.TryAddItem(itemData, quantity))
        {
            PlayPickupEffect();
            Destroy(gameObject);
        }
    }
    
    public void Initialize(ItemData data, int qty = 1)
    {
        itemData = data;
        quantity = qty;
    }
    
    private void PlayPickupEffect()
    {
        
    }
}