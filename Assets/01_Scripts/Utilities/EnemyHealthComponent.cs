using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyHealthComponent : MonoBehaviour
{
    private EnemyStatsSO _stats;
    private float _currentHealth;
    private float _regenDelayTimer;
    
    [Header("UI")]
    [SerializeField] private GameObject _healthBarContainer;
    private Slider         _slider;
    private Image          _fillImage;
    [SerializeField] private Gradient _gradient;

    public float Current    => _currentHealth;
    public float Percentage => _currentHealth / _stats.MaxHealth;
    public bool  IsAlive    => _currentHealth > 0f;
    public UnityEvent OnDeath;

    void Awake()
    {
        _stats = GetComponent<EnemyBase>().EnemyStats;
        _currentHealth   = _stats.MaxHealth;
        _regenDelayTimer = 0f;
        
        if (_healthBarContainer != null)
        {
            _slider    = _healthBarContainer.GetComponent<Slider>();
            _fillImage = _slider.fillRect.GetComponentInChildren<Image>();

            _slider.maxValue = _stats.MaxHealth;
            _slider.value    = _currentHealth;
            _fillImage.color = _gradient.Evaluate(Percentage);
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
        _slider.value    = _currentHealth;
        _fillImage.color = _gradient.Evaluate(Percentage);
    }

    public bool TryHeal(float amount)
    {
        _currentHealth = Mathf.Min(_stats.MaxHealth, _currentHealth + amount);
        _regenDelayTimer = _stats.HealthRegenDelay;
        return true;
    }
    
    public void TakeDamage(float damage)
    {
        if (_currentHealth <= 0f) 
            return; // already dead

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        _regenDelayTimer = _stats.HealthRegenDelay;
        UpdateUI();

        if (_currentHealth <= 0f)
            OnDeath?.Invoke();
    }
    
    private void HandleRegen(float dt)
    {
        if (_currentHealth <= 0f || _currentHealth >= _stats.MaxHealth)
            return;

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= dt;
            return;
        }

        _currentHealth = Mathf.Min(
            _stats.MaxHealth,
            _currentHealth + _stats.HealthRegenRate * dt
        );
    }
}
