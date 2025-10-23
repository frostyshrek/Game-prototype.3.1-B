using UnityEngine;

namespace BattleSystem
{
    public class EnemyController : CharacterController
    {
        [Header("enemy special attributes")]
        public BattleManager battleManager;

        void Start()
        {
            isPlayer = false;
            // InitializeCharacter();
        }

        // TODO: add player special attributes
    }
}