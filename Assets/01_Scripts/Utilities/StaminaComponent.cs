using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StaminaComponent : MonoBehaviour
{
    private PlayerStatsSO _stats;
    private float _currentStamina;
    private float _regenDelayTimer;
    
    [Header("UI")]
    [SerializeField] private GameObject _staminaBarContainer;

    private Slider _slider;
    private Image _fillImage;
    [SerializeField] private Gradient _gradient;

    public float Current       => _currentStamina;
    public bool  IsEmpty       => _currentStamina <= 0f;
    public float Percentage    => _currentStamina / _stats.MaxStamina;
    public UnityEvent OnDepleted;
    
    public bool CanSprint => _currentStamina >= 0;

    void Awake()
    {
        _stats = GetComponent<PlayerController>().Stats;
        _currentStamina = _stats.MaxStamina;
        _regenDelayTimer = 0f;

        if (_staminaBarContainer != null)
        {
            _slider    = _staminaBarContainer.GetComponent<Slider>();
            _fillImage = _slider.fillRect.GetComponentInChildren<Image>();
            _fillImage.color = _gradient.Evaluate(Percentage);
            _slider.maxValue = _stats.MaxStamina;
            _slider.value    = _currentStamina;
        }
    }

    void Update()
    {
        HandleRegen(Time.deltaTime);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (_slider == null) return;
        _slider.value = _currentStamina;
        _fillImage.color = _gradient.Evaluate(Percentage);
    }

    public bool TryUse(float amount)
    {
        if (_currentStamina < amount) return false;
        
        _currentStamina -= amount;
        _regenDelayTimer = _stats.StaminaRegenDelay;
        
        if (_currentStamina <= 0f)
        {
            _currentStamina = 0f;
            OnDepleted?.Invoke();
        }
        
        return true;
    }
    
    public bool CanAfford(float amount)
    {
        return _currentStamina >= amount;
    }

    private void HandleRegen(float dt)
    {
        if (_currentStamina >= _stats.MaxStamina)
            return;

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= dt;
            return;
        }

        _currentStamina = Mathf.Min(
            _stats.MaxStamina,
            _currentStamina + _stats.StaminaRegenRate * dt
        );
    }
}

