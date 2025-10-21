using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Character Stats")]
    public string characterName;
    public int maxHealth = 100;
    public int health = 100;

    [Header("Status Effects")]
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health < 0) health = 0;

        Debug.Log($"{characterName} took {amount} damage! Remaining HP: {health}");

        if (health == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;

        Debug.Log($"{characterName} healed for {amount} HP! Current HP: {health}");
    }

    private void Die()
    {
        Debug.Log($"{characterName} has been defeated!");
        // Optionally trigger animation, disable controls, etc.
    }

    public int CalculateOutgoingDamage(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in activeEffects)
        {
            modifiedDamage = effect.ModifyOutgoingDamage(modifiedDamage);
        }

        return Mathf.Max(0, modifiedDamage);
    }

    public int CalculateIncomingDamage(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in activeEffects)
        {
            modifiedDamage = effect.ModifyIncomingDamage(modifiedDamage);
        }

        return Mathf.Max(0, modifiedDamage);
    }

    public void ApplyEffect(StatusEffect effect)
    {
        StatusEffect newEffect = Instantiate(effect);
        newEffect.OnApply(this);
        activeEffects.Add(newEffect);

        Debug.Log($"{characterName} gained effect: {newEffect.effectName}");
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        if (activeEffects.Contains(effect))
        {
            activeEffects.Remove(effect);
            Debug.Log($"{characterName} lost effect: {effect.effectName}");
        }
    }

    public bool HasEffect(System.Type effectType)
    {
        foreach (var effect in activeEffects)
        {
            if (effect.GetType() == effectType)
                return true;
        }
        return false;
    }

    public void OnTurnStart()
    {
        foreach (var effect in new List<StatusEffect>(activeEffects))
        {
            effect.OnTurnStart(this);
        }
    }

    public void OnTurnEnd()
    {
        foreach (var effect in new List<StatusEffect>(activeEffects))
        {
            effect.OnTurnEnd(this);
        }
    }
}
