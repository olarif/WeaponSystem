using UnityEngine;
using TMPro;

public class InteractCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buttonText;
    
    private void ShowCanvas()
    {
        gameObject.SetActive(true);
    }
    
    private void HideCanvas()
    {
        gameObject.SetActive(false);
    }

    public void InitializeCanvas(string buttonText)
    {
        _buttonText.text = buttonText;
    }
    
    public void Hide()
    {
        HideCanvas();
    }
}
