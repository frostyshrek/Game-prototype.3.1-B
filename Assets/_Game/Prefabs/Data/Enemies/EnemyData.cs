using UnityEngine;



namespace BattleSystem
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Battle System/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string id;                 // e.g. "skeleton_foot_soldier"
        public string displayName;        // e.g. "Skeleton Foot Soldier"

        public GameObject battlePrefab;   // prefab to spawn in Battle scene
        public ArenaBiome arenaBiome;  // which Arena this enemy fights in

        // public EnemyAttackPattern[] attackPatterns;
        // public CardData[] cardDrops;
        // public int maxHealth;
    }
}
