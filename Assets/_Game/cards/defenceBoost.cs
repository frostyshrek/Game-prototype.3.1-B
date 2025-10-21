using UnityEngine;

public enum DefenceType
{
    multiplier,
    flat
}

[CreateAssetMenu(fileName = "DefenceBoost", menuName = "StatusEffects/DefenceBoost")]
public class DefenceBoost : StatusEffect
{
    public float boostAmount = 0.5f;
    public DefenceType defenceType = DefenceType.multiplier;
    public bool singleUse = true;
    private bool used = false;

    public override int ModifyIncomingDamage(int damage)
    {
        if (used && singleUse)
            return damage;

        int modifiedDamage = damage;

        if (defenceType == DefenceType.multiplier)
        {
            modifiedDamage = Mathf.RoundToInt(damage * (1 - boostAmount));
        }
        else if (defenceType == DefenceType.flat)
        {
            modifiedDamage = damage - Mathf.RoundToInt(boostAmount);
            if (modifiedDamage < 0)
                modifiedDamage = 0;
        }

        if (singleUse)
            used = true;

        return modifiedDamage;
    }

    public override void OnTurnStart(Character target)
    {
        if (used || --duration <= 0)
        {
            target.RemoveStatusEffect(this);
            Debug.Log($"{target.characterName}'s Defence Boost has worn off.");
        }
    }   
}