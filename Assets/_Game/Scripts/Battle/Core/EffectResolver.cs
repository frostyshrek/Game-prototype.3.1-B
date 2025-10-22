using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class EffectResolver : MonoBehaviour
    {
        public BattleManager battleManager;
        public CardManager cardManager;

        [Header("current turn status")]
        public CardAttribute currentGlobalAttribute = CardAttribute.None;
        public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();

        // 解析并执行暂存区所有卡牌效果
        // resolve and execute every card in preparation area
        public IEnumerator ResolvePreparationEffects(List<CardData> preparationCards)
        {
            Debug.Log($"resolving card number: {preparationCards.Count}");

            // resit status
            currentGlobalAttribute = CardAttribute.None;
            
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
            CardAttribute attribute = effect.attribute;

            // check if is global
            if (currentGlobalAttribute != CardAttribute.None)
            {
                attribute = currentGlobalAttribute;
            }

            // state effect correction (can be extended)
            damage = CalculateFinalDamage(damage, attribute);

            // apply damage to target
            if (effect.target == EffectTarget.Enemy || effect.target == EffectTarget.Both)
            {
                battleManager.enemyHealth -= damage;
                Debug.Log($"deal {damage} {attribute}attribute damage to enemy, enemy hp: {battleManager.enemyHealth}");
            }

            if (effect.target == EffectTarget.Self || effect.target == EffectTarget.Both)
            {
                battleManager.playerHealth -= damage;
                Debug.Log($"deal {damage} {attribute}attribute damage, player hp: {battleManager.playerHealth}");
            }

            yield return null;
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
                battleManager.playerHealth = Mathf.Min(battleManager.playerHealth + healAmount, battleManager.maxHealth);
                Debug.Log($"player heals {healAmount} hp, player hp: {battleManager.playerHealth}");
            }

            if (effect.target == EffectTarget.Enemy || effect.target == EffectTarget.Both)
            {
                battleManager.enemyHealth = Mathf.Min(battleManager.enemyHealth + healAmount, battleManager.maxHealth);
                Debug.Log($"enemy heals {healAmount} hp, enemy hp: {battleManager.enemyHealth}");
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
            StatusEffect newStatus = new StatusEffect
            {
                type = effect.statusType,
                duration = effect.statusDuration,
                target = effect.target
            };
            
            activeStatusEffects.Add(newStatus);
            Debug.Log($"apply status effect: {effect.statusType}, duration: {effect.statusDuration} rounds");
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
            // TODO: Attribute conflict logic applied here
            return baseDamage;
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
    }

    // status effect category
    [System.Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public int duration;
        public EffectTarget target;
    }
}