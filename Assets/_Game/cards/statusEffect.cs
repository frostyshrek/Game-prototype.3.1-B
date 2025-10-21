using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public string effectName;
    public float duration;

    public virtual void OnApply(Character target) { }
    public virtual void OnRemove(Character target) { }
    public virtual void OnTurnStart(Character target) { }
    public virtual void OnTurnEnd(Character target) { }

    public virtual int ModifyOutgoingDamage(int damage) => damage;
    public virtual int ModifyIncomingDamage(int damage) => damage;
}