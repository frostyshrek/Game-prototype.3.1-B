using UnityEngine;

namespace BattleSystem
{
    public enum ArenaBiome
    {
        Castle,
        Fire,
        Ice,
        Forest,
        Lightning,
        FinalBoss
    }

    /// <summary>
    /// Spawns the correct arena prefab for the current enemy/biome.
    /// Lives in the Battle scene.
    /// </summary>
    public class ArenaManager : MonoBehaviour
    {
        [Header("Spawn / Parent")]
        [Tooltip("Where the arena root will be instantiated (usually 0,0,0).")]
        public Transform arenaRoot;          // can be empty at world origin

        [Header("Arena Prefabs")]
        public GameObject arenaCastle;
        public GameObject arenaFire;
        public GameObject arenaIce;
        public GameObject arenaForest;
        public GameObject arenaLightning;
        public GameObject arenaFinalBoss;

        // runtime
        private GameObject _currentArenaInstance;

        /// <summary>
        /// Call this from BattleManager once you know which enemy is fighting.
        /// </summary>
        public void SetupForEnemy(EnemyData enemyData)
        {
            if (enemyData == null)
            {
                Debug.LogWarning("ArenaManager.SetupForEnemy called with null EnemyData");
                return;
            }

            SpawnArenaForBiome(enemyData.arenaBiome);
        }

        public void SpawnArenaForBiome(ArenaBiome biome)
        {
            // Destroy previous arena if we had one (e.g. reload battle)
            if (_currentArenaInstance != null)
            {
                Destroy(_currentArenaInstance);
                _currentArenaInstance = null;
            }

            GameObject prefab = ChooseArenaPrefab(biome);
            if (prefab == null)
            {
                Debug.LogWarning($"ArenaManager: No prefab assigned for biome {biome}");
                return;
            }

            Transform parent = arenaRoot != null ? arenaRoot : transform;
            _currentArenaInstance = Instantiate(prefab, parent.position, parent.rotation, parent);

            Debug.Log($"ArenaManager: Spawned arena {prefab.name} for biome {biome}");
        }

        private GameObject ChooseArenaPrefab(ArenaBiome biome)
        {
            switch (biome)
            {
                case ArenaBiome.Castle:      return arenaCastle;
                case ArenaBiome.Fire:        return arenaFire;
                case ArenaBiome.Ice:         return arenaIce;
                case ArenaBiome.Forest:      return arenaForest;
                case ArenaBiome.Lightning:   return arenaLightning;
                case ArenaBiome.FinalBoss:   return arenaFinalBoss;
                default:                     return arenaCastle;
            }
        }
    }
}
