using UnityEngine;

namespace BattleSystem
{
    public class EnemyController : BattleCharacter
    {
        [Header("enemy special attributes")]
        public BattleManager battleManager;

        [Header("Floating HP")]
        public GameObject hpUIPrefab;     // drag WorldSpaceHP prefab
        private WorldSpaceHPUI hpUI;      // runtime instance

        void Start()
        {
            isPlayer = true;
            InitializeCharacter(); // base Start won't auto-run, so we call it here

            // if (hpUIPrefab != null)
            // {
            //     var ui = Instantiate(hpUIPrefab);
            //     hpUI = ui.GetComponent<WorldSpaceHPUI>();
            //     hpUI.target = transform;
            //     hpUI.SetHP(currentHealth, maxHealth);
            // }

            // subscribe to health change updates
            OnHealthChanged += HandleHealthChanged;
        }

        void OnDestroy()
        {
            OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(int cur, int max)
        {
            // hpUI?.SetHP(cur, max);
        }
    }
}
