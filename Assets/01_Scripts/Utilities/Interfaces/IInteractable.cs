using UnityEngine;

public interface IInteractable
{
    string GetPromptText();
    void CanInteract(WeaponManager weaponManager);
    void OnInteract();
    void Highlight(bool isHighlighted);
}