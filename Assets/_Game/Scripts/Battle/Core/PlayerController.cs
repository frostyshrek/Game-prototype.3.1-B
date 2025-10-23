using UnityEngine;

namespace BattleSystem
{
    public class PlayerController : CharacterController
    {
        [Header("player special attributes")]
        public CardManager cardManager;
        public BattleManager battleManager;

        void Start()
        {
            isPlayer = true;
            InitializeCharacter();
        }

        

        // TODO: add player special attributes
    }
}