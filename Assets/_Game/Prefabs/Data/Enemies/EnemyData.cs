using UnityEngine;

public enum EnemyTier
{
    Normal,
    MiniBoss,
    MainBoss
}

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Battle System/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string id;                 // e.g. "skeleton_foot_soldier"
        public string displayName;        // e.g. "Skeleton Foot Soldier"

        public GameObject battlePrefab;   // prefab to spawn in Battle scene
        public ArenaBiome arenaBiome;  // which Arena this enemy fights in

        [Header("Rewards")]
        public EnemyTier tier = EnemyTier.Normal;

        [Tooltip("MiniBoss + MainBoss drop this card.")]
        public CardData droppedCard;

        [Tooltip("MainBoss only: drops a Great Rune.")]
        public GreatRune droppedRune;

        // public EnemyAttackPattern[] attackPatterns;
        // public CardData[] cardDrops;
        // public int maxHealth;
    }
}
