using UnityEngine;

public interface IInteractable
{
    void Interact(WeaponManager weaponManager);
    void Interact();
    void Highlight(bool isHighlighted, Camera playerCamera);
}