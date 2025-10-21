using UnityEngine;

public class CardManager : MonoBehaviour
{
    public void PlayCard(CardData card, Character player, Character enemy)
    {
        Debug.Log($"Playing card: {card.cardName}");

        switch (card.cardType)
        {
            case CardType.Attack:
                HandleAttackCard(card, player, enemy);
                break;

            case CardType.Defense:
                HandleDefenseCard(card, player);
                break;

            case CardType.Buff:
                HandleBuffCard(card, player);
                break;

            case CardType.Heal:
                HandleHealCard(card, player);
                break;
        }
    }

    private void HandleAttackCard(CardData card, Character player, Character enemy)
    {
        int baseDamage = card.powerValue;
        int outgoingDamage = player.CalculateOutgoingDamage(baseDamage);

        // Let the enemy reduce damage based on defence effects
        int finalDamage = enemy.CalculateIncomingDamage(outgoingDamage);

        enemy.TakeDamage(finalDamage);

        Debug.Log($"{player.characterName} attacked {enemy.characterName} for {finalDamage} damage!");

        // Apply attack-related status effect (like Burn, Poison, etc.)
        if (card.statusEffect != null)
        {
            StatusEffect effectInstance = Instantiate(card.statusEffect);
            enemy.ApplyEffect(effectInstance);
        }
    }

    private void HandleDefenseCard(CardData card, Character player)
    {
        if (card.statusEffect != null)
        {
            StatusEffect effectInstance = Instantiate(card.statusEffect);
            player.ApplyEffect(effectInstance);
            Debug.Log($"{player.characterName} increased defense with {effectInstance.effectName}!");
        }
        else
        {
            Debug.Log($"{player.characterName} defended but no effect attached.");
        }
    }

    private void HandleBuffCard(CardData card, Character player)
    {
        if (card.statusEffect != null)
        {
            StatusEffect effectInstance = Instantiate(card.statusEffect);
            player.ApplyEffect(effectInstance);
            Debug.Log($"{player.characterName} gained buff: {effectInstance.effectName}");
        }
    }

    private void HandleHealCard(CardData card, Character player)
    {
        player.Heal(card.powerValue);
        Debug.Log($"{player.characterName} healed for {card.powerValue} HP!");
    }
}
