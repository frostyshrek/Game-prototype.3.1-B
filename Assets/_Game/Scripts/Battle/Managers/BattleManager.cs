using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BattleSystem
{
    public class BattleManager : MonoBehaviour
    {
        public CardManager cardManager;
        public EffectResolver effectResolver;
        public PlayerController playerController;
        public EnemyController enemyController;

        [Header("Player References")]
        public EnemyAttackController enemyAttackController;   // attach on Enemy
        public BattleOrbitMovement playerMovement;            // attach on Player
        public PlayerEnergy playerEnergy;                     // attach on Player

        [Header("Battle Loadout")]
        [Tooltip("5 cards the player has chosen before entering battle")]
        [SerializeField] private List<CardData> equippedBattleCards = new List<CardData>();  // set to 5 cards in Inspector
        [SerializeField] private int cardsPerHand = 3;

        [Header("Enemy setup")]
        public Transform enemySpawnPoint;
        public TMP_Text enemyNameText;

        [Header("UI")]
        [SerializeField] private HandUIController handUI;
        [SerializeField] private CastButtonUI castButtonUI;
        [SerializeField] private GameOverUI gameOverUI;

        [Header("Enemy HP UI")]
        public Slider enemyHealthSlider;
        public TMP_Text enemyHealthText;
        // public GameObject enemyDamageTextPrefab; // maybe in the future: floating damage text

        [Header("Enemy Telegraph UI")]
        public EnemyTelegraphUI enemyTelegraphUI;

        [Header("Turn Control")]
        public bool isPlayerTurn = true;
        public int cardsPlayedThisTurn = 0;
        public BattleState currentState = BattleState.PlayerTurn;

        public enum BattleState
        {
            PlayerTurn,
            ResolvingEffects,
            GameOver
        }

        void Start()
        {
            InitializeBattle();
            SetupEnemyFromGameState();

            // enemy starts their own continuous attack loop
            if (enemyAttackController != null)
            {
                enemyAttackController.BeginAttacks();
            }

            StartPlayerTurn();
        }

        void InitializeBattle()
        {
            cardManager.playerHand.Clear();
            cardManager.preparationArea.Clear();
            cardManager.discardPile.Clear();
            cardManager.drawPile.Clear();

            if (handUI == null)
            {
                handUI = FindObjectOfType<HandUIController>(true);
            }

            BuildRandomHandFromEquipped();
            Debug.Log($"Battle init: hand={cardManager.playerHand.Count} cards (from {equippedBattleCards.Count} equipped)");
        }

        private void BuildRandomHandFromEquipped()
        {
            cardManager.playerHand.Clear();
            cardManager.preparationArea.Clear();

            if (equippedBattleCards == null || equippedBattleCards.Count == 0)
            {
                Debug.LogWarning("No equippedBattleCards set! Please assign 5 in BattleManager.");
                return;
            }

            List<CardData> pool = new List<CardData>(equippedBattleCards);

            for (int i = 0; i < cardsPerHand && pool.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, pool.Count);
                CardData chosen = pool[randomIndex];
                pool.RemoveAt(randomIndex);

                cardManager.playerHand.Add(chosen);
            }

            if (handUI != null)
                handUI.RebuildHand(cardManager.playerHand);
        }

        // ------------------- PLAYER TURN (card selection cycle) -------------------

        public void StartPlayerTurn()
        {
            isPlayerTurn = true;
            currentState = BattleState.PlayerTurn;
            cardsPlayedThisTurn = 0;

            effectResolver.ProcessDamageOverTimeEffects(EffectTarget.Self);

            if (playerMovement != null)
                playerMovement.SetCanMove(true);   // movement always allowed except GameOver

            if (playerController.IsDead())
            {
                GameOver(false);
                Debug.Log("Player hp below 0, lose");
                return;
            }
            
            Debug.Log("=== Player's card selection starts ===");
        }

        public void EndPlayerTurn()
        {
            if (currentState != BattleState.PlayerTurn) return;

            // only allow ending the "turn" if at least one card was actually played
            if (cardManager.preparationArea == null || cardManager.preparationArea.Count == 0)
            {
                Debug.Log("Cannot Cast: no cards selected.");

                if (castButtonUI != null)
                    castButtonUI.FlashError();

                return;
            }

            // DO NOT affect movement or enemy attacks here
            // Enemy is always attacking independently

            // enforce hand size if ever needed
            while (cardManager.IsHandOverLimit())
            {
                cardManager.DiscardRandomHandCard();
            }

            Debug.Log("Player's selection ends, resolving card effects");
            StartCoroutine(ResolveCardEffects());
        }

        // ------------------- RESOLVE CARD EFFECTS -------------------

        IEnumerator ResolveCardEffects()
        {
            currentState = BattleState.ResolvingEffects;
            
            List<CardData> cardsToResolve = cardManager.GetPreparationCards();
            Debug.Log($"Resolving effects: {cardsToResolve.Count} cards");

            yield return StartCoroutine(effectResolver.ResolvePreparationEffects(cardsToResolve));
    
            cardManager.DiscardPreparationArea();

            if (enemyController.currentHealth <= 0)
            {
                GameOver(true);
                yield break;
            }

            // Immediately start next card-selection cycle with a new random hand
            BuildRandomHandFromEquipped();
            StartPlayerTurn();
        }

        // ------------------- GAME OVER -------------------

        public void GameOver(bool playerWon)
        {
            if (currentState == BattleState.GameOver) return;

            currentState = BattleState.GameOver;
            Debug.Log(playerWon ? "[BattleManager] WIN" : "[BattleManager] LOSE");

            if (playerMovement != null)
                playerMovement.SetCanMove(false);
            if (enemyAttackController != null)
                enemyAttackController.StopAttacks();

            if (playerWon && GameState.I != null)
            {
                GameState.I.MarkEncounterDefeated(GameState.I.LastEncounterId);
                GameState.I.GiveKey(KeyItem.AncientKey);
            }

            if (gameOverUI != null)
            {
                if (playerWon)
                    gameOverUI.ShowWin();
                else
                    gameOverUI.ShowDeath();
            }
            else
            {
                Debug.LogWarning("[BattleManager] gameOverUI not assigned, falling back to direct scene load");
                StartCoroutine(ReturnToGladeAfterDelay(1.5f));
            }
        }

        IEnumerator ReturnToGladeAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("Glade");
        }

        public void OnEndTurnButtonClicked()
        {
            if (currentState == BattleState.PlayerTurn)
            {
                EndPlayerTurn();
            }
        }

        void Update()
        {
            if (currentState == BattleState.PlayerTurn)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    OnEndTurnButtonClicked();
                }
            }
        }

        void SetupEnemyFromGameState()
        {
            var gs = GameState.I;
            EnemyData data = (gs != null) ? gs.CurrentEncounter : null;
            if (data == null)
            {
                Debug.LogWarning("No EnemyData in GameState; cannot spawn enemy.");
                return;
            }

            Vector3 pos = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            Quaternion rot = enemySpawnPoint != null ? enemySpawnPoint.rotation : Quaternion.identity;

            GameObject enemyGO = Instantiate(data.battlePrefab, pos, rot);

            enemyController = enemyGO.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                Debug.LogError("Spawned enemy prefab has no EnemyController!");
                return;
            }

            // HP UI
            enemyController.healthBarSlider = enemyHealthSlider;
            enemyController.healthText = enemyHealthText;
            // enemyController.damageTextPrefab = enemyDamageTextPrefab;
            enemyController.InitializeCharacter();

            // Hook enemy into other systems
            if (effectResolver != null)
                effectResolver.enemyController = enemyController;
                
            // Hook Enemy Attack Controller wiring
            var attackCtrl = enemyGO.GetComponent<EnemyAttackController>();
            if (attackCtrl != null)
            {
                attackCtrl.playerMovement = playerMovement;
                attackCtrl.playerEnergy = playerEnergy;
                attackCtrl.playerCharacter = playerController;
                attackCtrl.battleManager = this;
                attackCtrl.telegraphUI = enemyTelegraphUI;
            }

            // Hook Name text
            if (enemyNameText != null)
                enemyNameText.text = data.displayName;
        }
    }
}
