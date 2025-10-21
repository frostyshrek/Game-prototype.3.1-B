using UnityEngine;

public enum BoostType
{
    multiplier,
    flat
}

[CreateAssetMenu(fileName = "AttackBoost", menuName = "StatusEffects/AttackBoost")]
public class AttackBoost : StatusEffect
{
    public float boostAmount = 2f;
    public BoostType boostType = BoostType.multiplier;
    public bool singleUse = true;
    private bool used = false;

    public override int ModifyOutgoingDamage(int damage)
    {
        if (used && singleUse)
            return damage;

        int modifiedDamage = damage;

        if (boostType == BoostType.multiplier)
        {
            modifiedDamage = Mathf.RoundToInt(damage * boostAmount);
        }
        else if (boostType == BoostType.flat)
        {
            modifiedDamage = damage + Mathf.RoundToInt(boostAmount);
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
            Debug.Log($"{target.charactername}'s Attack Boost has worn off.");
        }
    }   
}