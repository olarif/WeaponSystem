public interface IPickupable
{
    string GetPromptText();
    bool CanPickup(PlayerInventory inventory);
    void OnPickup(PlayerInventory inventory);
    void Highlight(bool highlighted);
}