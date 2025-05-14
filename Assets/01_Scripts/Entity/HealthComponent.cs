using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth);
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => _maxHealth;
    
    public void Heal(int amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        //broadcast heal event for UI etc.
        OnHealthChanged?.Invoke(_currentHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }
    
    public bool IsAlive() => _currentHealth > 0;

    public void TakeDamage(float damage)
    {
         Debug.Log($"Applying {damage} damage to {gameObject.name}");
        
        _currentHealth -= Mathf.RoundToInt(damage);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        OnHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
}
