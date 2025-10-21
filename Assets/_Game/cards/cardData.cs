using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite artwork;
    public CardType type;
    public int attack;
    public StatusEffect statusEffect;
    public string description;
}

public enum CardType
{
    Attack,
    Defense,
    Buff,
    Heal
}   