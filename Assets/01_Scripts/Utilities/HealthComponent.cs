using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] float _maxHealth = 100;
    private float _currentHealth;

    public float GetMaxHealth() => _maxHealth;
    public float GetCurrentHealth => _currentHealth;
    public float GetCurrentHealthPercentage => _currentHealth / _maxHealth;
    
    public GameObject healthBar;
    public Slider     slider;
    public Gradient   gradient;
    
    public UnityEvent OnDeath;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }
    
    void Start()
    {
        if (healthBar != null)
        {
            slider = healthBar.GetComponent<Slider>();
            slider.maxValue = _maxHealth;
            slider.value = _currentHealth;
            slider.fillRect.GetComponentInChildren<Image>().color = gradient.Evaluate(slider.normalizedValue);
        }
    }
    
    void UpdateBar()
    {
        if (slider == null) return;
        slider.value = _currentHealth;
        slider.fillRect.GetComponentInChildren<Image>().color = gradient.Evaluate(slider.normalizedValue);
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        UpdateBar();
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }
    
    public bool IsAlive() => _currentHealth > 0;

    public void TakeDamage(float damage)
    {
         Debug.Log($"Applying {damage} damage to {gameObject.name}");

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        UpdateBar();
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
}
