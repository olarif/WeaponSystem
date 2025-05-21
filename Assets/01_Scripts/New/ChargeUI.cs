using UnityEngine;
using UnityEngine.UI;

public class ChargeUI : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;
    [Tooltip("CanvasGroup to fade in/out")]
    [SerializeField] CanvasGroup canvasGroup = null;

    public Gradient colorGradient;

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
        if (t < 0f || t > 1f) return;

        slider.value = t;
        fillImage.color = colorGradient.Evaluate(t);

        if (t >= 1f)
            Hide();
        else
            Show();
    }
}