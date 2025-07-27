using UnityEngine;

[System.Serializable]
public class ItemData : MonoBehaviour
{
    public string itemName;
    public string description;
    public Sprite icon;
    public PickupType type;
    public int maxStackSize = 1;
    public bool isConsumable;
}