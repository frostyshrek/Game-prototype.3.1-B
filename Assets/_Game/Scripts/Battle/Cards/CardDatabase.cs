using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Battle System/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        public List<CardData> allCards = new List<CardData>();

        public CardData FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return allCards.Find(c => c != null && c.cardId == id);
        }
    }
}
