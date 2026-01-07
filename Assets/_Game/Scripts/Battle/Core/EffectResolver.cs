using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class EffectResolver : MonoBehaviour
    {
        public BattleManager battleManager;
        public CardManager cardManager;
        public CharacterController characterController;
        public PlayerController playerController;
        public EnemyController enemyController;

        [Header("Runtime Enemy Data")]
        public ArenaBiome currentEnemyBiome = ArenaBiome.Castle;

        [Header("UI Feedback")]
        public BattleFeedbackUI feedbackUI;
        [SerializeField] private BattleSFX battleSFX;

        [Header("Animation")]
        public Animator playerAnimator;

        [Header("current turn status")]
        public CardAttribute currentGlobalAttribute = CardAttribute.Physical;
        public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();


        // resolve and execute every card in preparation area
        public IEnumerator ResolvePreparationEffects(List<CardData> preparationCards)
        {
            Debug.Log($"resolving card number: {preparationCards.Count}");

            // resit status
            currentGlobalAttribute = CardAttribute.Physical;

            // store sequence the card been executed
            List<CardEffect> effectsToExecute = new List<CardEffect>();

            // collect all effects
            foreach (CardData card in preparationCards)
            {
                foreach (CardEffect effect in card.effects)
                {
                    effectsToExecute.Add(effect);
                }
            }

            // execute effects
            foreach (CardEffect effect in effectsToExecute)
            {
                yield return StartCoroutine(ExecuteEffect(effect));
                yield return new WaitForSeconds(0.3f); // interval between effects
            }

            // clean efects of the turn 
            CleanupTurnEffects();

            Debug.Log("finish resolving every effect");
        }
        

        // execute effect
        private IEnumerator ExecuteEffect(CardEffect effect)
        {
            Debug.Log($"effect type: {effect.effectType}, value: {effect.value}, target: {effect.target}");

            switch (effect.effectType)
            {
                case CardEffectType.Damage:
                    yield return StartCoroutine(ApplyDamage(effect));
                    break;

                case CardEffectType.Heal:
                    yield return StartCoroutine(ApplyHeal(effect));
                    break;

                case CardEffectType.ChangePreviousAttribute:
                    ChangePreviousAttribute(effect);
                    break;

                case CardEffectType.ChangeNextAttribute:
                    ChangeNextAttribute(effect);
                    break;

                case CardEffectType.SetGlobalAttribute:
                    SetGlobalAttribute(effect);
                    break;

                case CardEffectType.ApplyBuff:
                case CardEffectType.ApplyDebuff:
                    ApplyStatusEffect(effect);
                    break;

                case CardEffectType.BlockHeal:
                    ApplyHealBlock(effect);
                    break;

                case CardEffectType.DrawCard:
                    ApplyDrawCard(effect);
                    break;
            }
        }

        // apply damage
        private IEnumerator ApplyDamage(CardEffect effect)
        {
            int damage = effect.value;

            // start with the card's attribute
            CardAttribute attackAttr = effect.attribute;

            // global override
            if (currentGlobalAttribute != CardAttribute.Physical)
                attackAttr = currentGlobalAttribute;

            // status effects correction
            damage = CalculateFinalDamage(damage, attackAttr);

            // apply damage to enemy
            if (effect.target == EffectTarget.Enemy || effect.target == EffectTarget.Both)
            {
                // Play player attack animation
                if (playerAnimator != null)
                    playerAnimator.SetTrigger("Attack");

                // defender attribute derived from biome
                CardAttribute enemyAttr = GetEnemyAttributeFromBiome(currentEnemyBiome);

                float mult = GetAttributeMultiplier(attackAttr, enemyAttr);

                if (feedbackUI != null) {
                    if (mult >= 1.25f) 
                    {
                        feedbackUI.Show("SUPER EFFECTIVE!", FeedbackType.Effective, 1.4f);
                        battleSFX?.PlaySuperEffective();
                    }
                    else if (mult <= 0.85f){
                        feedbackUI.Show("NOT VERY EFFECTIVE…", FeedbackType.NotEffective, 1.4f);
                        battleSFX?.PlayNotEffective();
                    }
                    else battleSFX?.PlayHit();
                }

                int finalDamage = Mathf.RoundToInt(damage * mult);

                enemyController.TakeDamage(finalDamage);

                string label = GetEffectivenessLabel(mult);
                Debug.Log($"deal {finalDamage} ({damage} x{mult}) {attackAttr} vs {enemyAttr}. {label}");
            }

            // apply damage to self (rare, but supported)
            if (effect.target == EffectTarget.Self || effect.target == EffectTarget.Both)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"deal {damage} {attackAttr} to player, player hp: {playerController.currentHealth}");
            }

            yield return null;
        }

        private CardAttribute GetEnemyAttributeFromBiome(ArenaBiome biome)
        {
            switch (biome)
            {
                case ArenaBiome.Castle:    return CardAttribute.Physical;
                case ArenaBiome.Forest:    return CardAttribute.Earth;
                case ArenaBiome.Fire:      return CardAttribute.Fire;
                case ArenaBiome.Ice:       return CardAttribute.Ice;
                case ArenaBiome.Lightning: return CardAttribute.Lightning;

                // If you have a Void attribute, use it. If not, keep Physical.
                case ArenaBiome.FinalBoss: return CardAttribute.Physical;

                default: return CardAttribute.Physical;
            }
        }

        // apply heal
        private IEnumerator ApplyHeal(CardEffect effect)
        {
            int healAmount = effect.value;

            // check if there is heal block
            if (IsHealBlocked(effect.target))
            {
                Debug.Log("heal been block!");
                yield break;
            }

            if (effect.target == EffectTarget.Self || effect.target == EffectTarget.Both)
            {
                // battleManager.playerHealth = Mathf.Min(battleManager.playerHealth + healAmount, battleManager.maxHealth);
                playerController.Heal(healAmount);
                Debug.Log($"player heals {healAmount} hp, player hp: {playerController.currentHealth}");
            }

            if (effect.target == EffectTarget.Enemy || effect.target == EffectTarget.Both)
            {
                enemyController.Heal(healAmount);
                Debug.Log($"enemy heals {healAmount} hp, enemy hp: {enemyController.currentHealth}");
            }

            yield return null;
        }

        // apply attribute change (affect previous card only)
        private void ChangePreviousAttribute(CardEffect effect)
        {
            // TODO: track previous card and change its attribute
            Debug.Log($"change previous card attribute to: {effect.attribute}");
        }

        private void ChangeNextAttribute(CardEffect effect)
        {
            // TODO: track next card and change its attribute
            Debug.Log($"change next card attribute to: {effect.attribute}");
        }

        // set global attribute
        private void SetGlobalAttribute(CardEffect effect)
        {
            currentGlobalAttribute = effect.attribute;
            Debug.Log($"set global attribute to: {effect.attribute}");
        }

        // apply staus effect
        private void ApplyStatusEffect(CardEffect effect)
        {
            StatusEffect newStatus = new StatusEffect();
            newStatus.type = effect.statusType;
            newStatus.duration = effect.statusDuration;
            newStatus.target = effect.target;
            newStatus.value = effect.value;
            newStatus.attribute = effect.attribute;
            newStatus.displayName = GetStatusEffectDisplayName(effect.statusType, effect.attribute);

            activeStatusEffects.Add(newStatus);
            Debug.Log($"apply status effect: {effect.statusType}, duration: {effect.statusDuration} rounds, deals {effect.value} each round");
        }

        // apply continuous damage (called when every round started)
        public void ProcessDamageOverTimeEffects(EffectTarget turnOwner)
        {
            List<StatusEffect> effectsToRemove = new List<StatusEffect>();
            foreach (StatusEffect status in activeStatusEffects)
            {
                if (status.target == turnOwner || status.target == EffectTarget.Both)
                {
                    switch (status.type)
                    {
                        case StatusEffectType.DamageOverTime:
                            ApplyContinuousDamage(status, turnOwner);
                            break;
                            // other status effect type
                    }

                    // reduce duration
                    status.duration--;

                    // mark effects that need to be removed
                    if (status.duration <= 0)
                    {
                        effectsToRemove.Add(status);
                    }
                }
            }

            // remove expired effects
            foreach (StatusEffect expiredEffect in effectsToRemove)
            {
                activeStatusEffects.Remove(expiredEffect);
                Debug.Log($"effect: {expiredEffect.type} expired");
            }
        }

        // apply continuous dmamge
        private void ApplyContinuousDamage(StatusEffect dotEffect, EffectTarget target)
        {
            int damage = dotEffect.value;
            CardAttribute damageAttribute = dotEffect.attribute;
            string effectName = GetContinuousDamageName(damageAttribute);

            if (target == EffectTarget.Enemy || target == EffectTarget.Both)
            {
                enemyController.TakeDamage(damage);
                Debug.Log($"{effectName} deals {damage} damage to enemy, enemy hp: {enemyController.currentHealth}");
            }

            if (target == EffectTarget.Self || target == EffectTarget.Both)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"{effectName} deals {damage} damage to player, player hp: {playerController.currentHealth}");
            }

            Debug.Log($"{effectName} duration: {dotEffect.duration} rounds");
        }



        // apply heal block
        private void ApplyHealBlock(CardEffect effect)
        {
            StatusEffect healBlock = new StatusEffect
            {
                type = StatusEffectType.BlockNextHeal,
                duration = 1, // lasts once only
                target = effect.target
            };

            activeStatusEffects.Add(healBlock);
            Debug.Log($"{effect.target}'s heal been blocked");
        }

        // apply draw card
        private void ApplyDrawCard(CardEffect effect)
        {
            for (int i = 0; i < effect.value; i++)
            {
                CardData drawnCard = cardManager.DrawSingleCard();
                if (drawnCard != null)
                {
                    cardManager.playerHand.Add(drawnCard);
                    Debug.Log($"draw card: {drawnCard.cardName}");
                }
            }
        }

        // Calculate final damage (considering attribute conflict, etc.)
        private int CalculateFinalDamage(int baseDamage, CardAttribute attribute)
        {
            int damage = baseDamage;

            // Flat modifiers first
            for (int i = 0; i < activeStatusEffects.Count; i++)
            {
                var s = activeStatusEffects[i];
                if (s.type == StatusEffectType.DamageModifier && (s.target == EffectTarget.Self || s.target == EffectTarget.Both))
                {
                    damage += s.value;
                    // If the modifier should last multiple hits/turns, control via s.duration elsewhere.
                    // We just apply it here; duration is reduced in CleanupTurnEffects.
                }
            }

            // One-shot: DoubleNextDamage (consume 1 stack immediately)
            for (int i = 0; i < activeStatusEffects.Count; i++)
            {
                var s = activeStatusEffects[i];
                if (s.type == StatusEffectType.DoubleNextDamage && (s.target == EffectTarget.Self || s.target == EffectTarget.Both))
                {
                    damage *= 2;
                    s.duration -= 1; // consume now
                    if (s.duration <= 0) activeStatusEffects.RemoveAt(i--);
                    break; // only one double per hit
                }
            }

            // You can also factor 'attribute' here if you add elemental multipliers later

            return Mathf.Max(0, damage);
        }

        // check is heal been blocked
        private bool IsHealBlocked(EffectTarget target)
        {
            foreach (StatusEffect status in activeStatusEffects)
            {
                if (status.type == StatusEffectType.BlockNextHeal &&
                    (status.target == target || status.target == EffectTarget.Both))
                {
                    return true;
                }
            }
            return false;
        }

        // clean up effects
        private void CleanupTurnEffects()
        {
            // Reduce duration of state effects and remove expired ones
            for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
            {
                activeStatusEffects[i].duration--;
                if (activeStatusEffects[i].duration <= 0)
                {
                    Debug.Log($"effect {activeStatusEffects[i].type} expired");
                    activeStatusEffects.RemoveAt(i);
                }
            }
        }

        // get status effect display name
        private string GetStatusEffectDisplayName(StatusEffectType effectType, CardAttribute effectAttribute)
        {
            if (effectType == StatusEffectType.DamageOverTime)
            {
                return GetContinuousDamageName(effectAttribute);
            }
            return effectType.ToString();
        }

        // get continuous damage name 
        private string GetContinuousDamageName(CardAttribute attribute)
        {
            switch (attribute)
            {
                case CardAttribute.Fire:
                    return "burn effect";
                default:
                    return "continuous damage effect";
            }
        }

        private float GetAttributeMultiplier(CardAttribute attack, CardAttribute defense)
        {
            // Physical is neutral against everything
            if (attack == CardAttribute.Physical) return 1f;

            // Strong/weak rules:
            // Fire > Ice, Fire < Lightning
            if (attack == CardAttribute.Fire)
            {
                if (defense == CardAttribute.Ice) return 1.5f;
                if (defense == CardAttribute.Lightning) return 0.75f;
                return 1f;
            }

            // Ice > Earth, Ice < Fire
            if (attack == CardAttribute.Ice)
            {
                if (defense == CardAttribute.Earth) return 1.5f;
                if (defense == CardAttribute.Fire) return 0.75f;
                return 1f;
            }

            // Earth > Lightning, Earth < Ice
            if (attack == CardAttribute.Earth)
            {
                if (defense == CardAttribute.Lightning) return 1.5f;
                if (defense == CardAttribute.Ice) return 0.75f;
                return 1f;
            }

            // Lightning > Fire, Lightning < Earth
            if (attack == CardAttribute.Lightning)
            {
                if (defense == CardAttribute.Fire) return 1.5f;
                if (defense == CardAttribute.Earth) return 0.75f;
                return 1f;
            }

            return 1f;
        }

        private string GetEffectivenessLabel(float mult)
        {
            if (mult >= 1.25f) return "SUPER EFFECTIVE!";
            if (mult <= 0.85f) return "NOT VERY EFFECTIVE…";
            return "";
        }

        // status effect category
        [System.Serializable]
        public class StatusEffect
        {
            public StatusEffectType type;          // effect type
            public int duration;                   // effect duration
            public EffectTarget target;            // effect target
            public int value;                      // effect value
            public CardAttribute attribute;        // effect attribute
            public string displayName;             // effect name

            public StatusEffect() {}
        }
    }
}