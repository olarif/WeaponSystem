using UnityEngine;
using UnityEngine.UI;

public class ChargeUI : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;
    [Tooltip("CanvasGroup to fade in/out")]
    [SerializeField] CanvasGroup canvasGroup = null;

    public Gradient colorGradient;
    
    public bool autoHideOnComplete = false;

    void Awake()
    {
        canvasGroup.alpha = 0f;
        slider.value = 0f;
    }

    public void Show()   => canvasGroup.alpha = 1f;
    public void Hide()   => canvasGroup.alpha = 0f;
    public void Reset()
    {
        slider.value = 0f;
        canvasGroup.alpha = 0f;
    }
    
    public void SetPercent(float t)
    {
        t = Mathf.Clamp01(t);
        slider.value = t;
        
        fillImage.color = colorGradient.Evaluate(t);
        
        if (autoHideOnComplete && t >= 1f)
            Hide();
        else
            Show();
    }
}