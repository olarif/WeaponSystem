using UnityEngine;
using TMPro;

public class DisplayWeaponStats : MonoBehaviour
{
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponDescriptionText;
    public GameObject statsPanel;
    
    public void DisplayStats(string weaponName, string weaponDescription)
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }
        
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponName;
        }
        
        if (weaponDescriptionText != null)
        {
            weaponDescriptionText.text = weaponDescription;
        }
    }
    
    public void ClearStats()
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = string.Empty;
        }
        
        if (weaponDescriptionText != null)
        {
            weaponDescriptionText.text = string.Empty;
        }
        
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
    }
}
