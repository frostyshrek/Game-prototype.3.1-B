using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    // Card effect enumeration
    public enum CardEffectType
    {
        // basic effect 
        Damage,              // deal damage
        Heal,                // heal
        DrawCard,            // draw card
        DiscardCard,         // discard card

        
        // attributes
        ChangePreviousAttribute,     // change attribute (effect one previous card only)
        ChangeNextAttribute,         // change attribute (effect one next card only)
        SetGlobalAttribute,  // change global attribute (effect all cards in this turn)
        
        // buffs
        ApplyBuff,           // apply buff 
        ApplyDebuff,         // apply debuff
        RemoveBuff,          // remove buff
        
        // special effects (for later use)
        ModifyDamage,        // change damage
        BlockHeal,           // block heal
        CopyEffect,          // copy effect
        ConditionalEffect    // conditional effect
    }

    // effect target
    public enum EffectTarget
    {
        Self,               // self
        Enemy,              // enemy
        Both,               // both
        PreviousCard,       // previous card
        NextCard,           // next card
        Global              // global
    }

    // status effect type (for later use)
    public enum StatusEffectType
    {
        None,
        BlockNextHeal,      // block next heal
        DamageModifier,     // change damage
        ReflectDamage       // rebound damage
    }

    // attribute type
    public enum CardAttribute
    {
        None,
        Fire,
        Water,
        Earth,
        Air
    }

    // effect for single card
    [Serializable]
    public class CardEffect
    {
        [Header("Basic Effect Setting")]
        public CardEffectType effectType;  // effect type
        public EffectTarget target;        // effect target 
        public int value;                  // effect value
        public CardAttribute attribute;    // attribute
        
        [Header("Status Effect Setting")]
        public StatusEffectType statusType; // status effect type
        public int statusDuration;          // status duration
        
        [Header("Condition Setting")]
        public CardAttribute requiredAttribute; // required attribute
        public bool requirePreviousCard;   // required previous card
        public bool requireNextCard;       // required next card
    }

    // card data
    [CreateAssetMenu(fileName = "New Card", menuName = "Battle System/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Basic info")]
        public string cardName;
        public string description;
        // public Sprite cardImage;

        [Header("Card effect")]
        public List<CardEffect> effects;

        [Header("battle attributes")]
        public CardAttribute baseAttribute = CardAttribute.None;

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(description))
                return description;

            string desc = "";
            foreach (var effect in effects)
            {
                switch (effect.effectType)
                {
                    case CardEffectType.Damage:
                        desc += $"deal {effect.value} damage";
                        if (effect.attribute != CardAttribute.None)
                            desc += $"({effect.attribute} attribute)";
                        desc += "\n";
                        break;
                        
                    case CardEffectType.Heal:
                        desc += $"heal {effect.value} hp\n";
                        break;
                        
                    case CardEffectType.ChangePreviousAttribute:
                        desc += $"Change the damage of previous card to {effect. attribute} attribute\n";
                        break;
                        
                    case CardEffectType.ChangeNextAttribute:
                        desc += $"Change the damage of next card to {effect.attribute} attribute \n";
                        break;

                    case CardEffectType.SetGlobalAttribute:
                        desc += $"Change the damage of this round to {effect. attribute} attribute\n";
                        break;
                        
                    case CardEffectType.BlockHeal:
                        desc += $"block enemy's next heal\n";
                        break;
                    
                    case CardEffectType.ApplyBuff:
        
                        
                    case CardEffectType.ApplyDebuff:
                        desc += $"Apply {effect.statusType}, lasts for ({effect.statusDuration} rounds)\n";
                        break;

                    case CardEffectType.RemoveBuff:
                        desc += $"Remove buff\n";
                        break;
                        
                    case CardEffectType.DrawCard:
                        desc += $"draw {effect.value} card\n";
                        break;
                }
            }
            return desc.Trim();
        }
    }
}