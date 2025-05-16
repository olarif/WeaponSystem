using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StaminaComponent : MonoBehaviour
{
    [SerializeField] float _maxStamina = 100f;
    float _currentStamina;

    public float GetMaxStamina()      => _maxStamina;
    public float GetCurrentStamina    => _currentStamina;
    public float GetStaminaPercentage => _currentStamina / _maxStamina;

    public GameObject staminaBar;
    public Slider     slider;
    public Gradient   gradient;

    public UnityEvent OnStaminaDepleted;

    void Awake()
    {
        _currentStamina = _maxStamina;
    }

    void Start()
    {
        if (staminaBar != null)
        {
            slider           = staminaBar.GetComponent<Slider>();
            slider.maxValue  = _maxStamina;
            slider.value     = _currentStamina;
            slider.fillRect.GetComponentInChildren<Image>().color    = gradient.Evaluate(slider.normalizedValue);
        }
    }

    void UpdateBar()
    {
        if (slider == null) return;
        slider.value = _currentStamina;
        slider.fillRect.GetComponentInChildren<Image>().color = gradient.Evaluate(slider.normalizedValue);
    }

    public void Consume(float amount)
    {
        _currentStamina = Mathf.Clamp(_currentStamina - amount, 0, _maxStamina);
        UpdateBar();
        if (_currentStamina <= 0) OnStaminaDepleted?.Invoke();
    }

    public void Restore(float amount)
    {
        _currentStamina = Mathf.Clamp(_currentStamina + amount, 0, _maxStamina);
        UpdateBar();
    }
}

