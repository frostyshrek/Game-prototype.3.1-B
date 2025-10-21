using UnityEngine;

[CreateAssetMenu(fileName = "AttackBoost", menuName = "StatusEffects/AttackBoost")]
public class AttackBoost : StatusEffect
{
    public float attackIncrease = 2f;

    public override int ModifyOutgoingDamage(int damage)
    {
        return Mathf.RoundToInt(damage * attackIncrease);
    }

    public override void OnApply(Character target)
    {
        Debug.Log($"{target.charactername} gains an attack boost of {attackIncrease} for {duration} turns.");
    }

    public override void OnRemove(Character target)
    {
        Debug.Log($"{target.charactername}'s attack boost has worn off.");
    }
}