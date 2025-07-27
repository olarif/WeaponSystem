using UnityEngine;
using UnityEngine.UI;

public class ConsolePlayerSettings : MonoBehaviour
{
    private PlayerStatsSO playerStats;
    public Toggle autoJumpToggle;
    
    
    private void Start()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        
        if (playerController != null)
        {
            playerStats = playerController.Stats;
        }
        
        autoJumpToggle.isOn = playerStats.CanAutoJump;
        autoJumpToggle.onValueChanged.AddListener(OnAutoJumpToggleChanged);
    }
    
    private void OnAutoJumpToggleChanged(bool isOn)
    {
        if (playerStats != null)
        {
            playerStats.CanAutoJump = isOn;
            Debug.Log($"Auto Jump set to: {isOn}");
        }
    }
}