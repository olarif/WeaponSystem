using UnityEngine;

public class InteractableObject : BaseInteractable
{
    [SerializeField] private string _itemName = "Item";

    public override void Interact()
    {
        Destroy(gameObject);
    }
}