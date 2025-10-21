using UnityEngine;

[CreateAssetMenu(fileName = "BurnEffect", menuName = "StatusEffects/Burn")]

public class BurnEffect : StatusEffect
{
    public int burnDamage = 3

    public override void OnTurnStart(Character target)
    {
        target.TakeDamage(burnDamage);
        duration--;
        Debug.Log($"{target.charactername} takes {burnDamage} burn damage. {duration} turns remaining.");

        if (duration <= 0)
        {
            target.RemoveStatusEffect(this);
        }
    }
}