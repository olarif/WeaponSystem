using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;

    public int GetMaxHealth() => _maxHealth;
    public int GetCurrentHealth => _currentHealth;
    public int GetCurrentHealthPercentage => Mathf.RoundToInt((float)_currentHealth / _maxHealth * 100);
    
    public GameObject healthBar;
    public Slider slider;
    public Gradient gradient;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }
    
    private void Start()
    {
        if (healthBar != null)
        {
            slider = healthBar.GetComponent<Slider>();
            slider.maxValue = _maxHealth;
            slider.value = _currentHealth;
            slider.fillRect.GetComponentInChildren<Image>().color = gradient.Evaluate(slider.normalizedValue);
        }
    }
    
    private void SetHealthBar()
    {
        if (healthBar != null)
        {
            slider.value = _currentHealth;
            slider.fillRect.GetComponentInChildren<Image>().color = gradient.Evaluate(slider.normalizedValue);
        }
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        SetHealthBar();
    }

    private void Die()
    {
        //OnDeath?.Invoke();
    }
    
    public bool IsAlive() => _currentHealth > 0;

    public void TakeDamage(float damage)
    {
         Debug.Log($"Applying {damage} damage to {gameObject.name}");
        
        _currentHealth -= Mathf.RoundToInt(damage);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        SetHealthBar();
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
}
