class StatusEffect
{  
    StatusEffectDataSO statusEffectData;
    private StatusEffectManager owner;

    private float timer, tickTimer;
    
    public bool IsComplete => timer >= statusEffectData.duration;
    
    public StatusEffect(StatusEffectDataSO statusEffectData, StatusEffectManager owner)
    {
        this.statusEffectData = statusEffectData;
        this.owner = owner;
    }
    
    public void Tick(float deltaTime)
    {
        timer += deltaTime;
        tickTimer += deltaTime;

        if (tickTimer >= statusEffectData.tickRate)
        {
            owner.GetComponent<IDamageable>().ApplyDamage(statusEffectData.tickDamage);
            tickTimer = 0f;
        }
    }
}