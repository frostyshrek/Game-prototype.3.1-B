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
        [Header("Core Systems")]
        public CardManager cardManager;
        public EffectResolver effectResolver;
        public PlayerController playerController;
        public EnemyController enemyController;
        public ArenaManager arenaManager;
        public BattleSFX battleSFX;

        [Header("Player References")]
        public EnemyAttackController enemyAttackController;   // attach on Enemy
        public BattleOrbitMovement playerMovement;            // attach on Player
        public PlayerEnergy playerEnergy;                     // attach on Player

        [Header("Battle Loadout")]
        [SerializeField] private int cardsPerHand = 3;
        [SerializeField] private CardDatabase cardDatabase;

        [Header("Enemy setup")]
        public Transform enemySpawnPoint;
        public TMP_Text enemyNameText;

        [Header("UI")]
        [SerializeField] private HandUIController handUI;
        [SerializeField] private CastButtonUI castButtonUI;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private BattleFeedbackUI feedbackUI;

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
            EnemyData enemyData = null;
            if (GameState.I != null)
                enemyData = GameState.I.CurrentEncounter;

            if (enemyData != null)
            {
                if (arenaManager != null)
                    arenaManager.SetupForEnemy(enemyData);
                else
                    Debug.LogWarning("BattleManager: arenaManager is not assigned");
            }
            else
            {
                Debug.LogWarning("BattleManager: CurrentEncounter is null (did you start from Glade?)");
            }

            if (effectResolver != null && enemyData != null)
                effectResolver.currentEnemyBiome = enemyData.arenaBiome;

            if (GameState.I != null)
                GameState.I.LoadCards();

            InitializeBattle();
            SetupEnemyFromGameState();

            // Wire feedback system
            if (effectResolver != null)
                effectResolver.feedbackUI = feedbackUI;

            if (cardManager != null)
                cardManager.feedbackUI = feedbackUI;

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
        }

        private void BuildRandomHandFromEquipped()
        {
            cardManager.playerHand.Clear();
            cardManager.preparationArea.Clear();

            if (GameState.I == null)
            {
                Debug.LogWarning("BattleManager: No GameState found. Cannot load equipped deck.");
                return;
            }

            if (cardDatabase == null)
            {
                Debug.LogError("BattleManager: cardDatabase not assigned! Drag your CardDatabase asset onto BattleManager.");
                return;
            }

            // Get the 5 equipped card IDs from GameState
            var ids = GameState.I.EquippedCardIds;
            if (ids == null || ids.Count == 0)
            {
                Debug.LogWarning("BattleManager: EquippedCardIds is empty. Did you save a deck in Glade?");
                return;
            }

            // Convert IDs -> CardData
            List<CardData> equipped = new List<CardData>();
            foreach (var id in ids)
            {
                var card = cardDatabase.FindById(id);
                if (card != null) equipped.Add(card);
                else Debug.LogWarning($"BattleManager: Could not find CardData for id '{id}' in CardDatabase.");
            }

            if (equipped.Count == 0)
            {
                Debug.LogWarning("BattleManager: No valid equipped cards found.");
                return;
            }

            // Build random hand from equipped cards
            List<CardData> pool = new List<CardData>(equipped);

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

                    battleSFX?.PlayNotEffective();
                    feedbackUI?.Show("NO CARDS SELECTED", FeedbackType.Error, 1.6f);

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
                // Special case: final boss has its own death sequence
                var finalBoss = enemyController.GetComponent<FinalBossVisuals>();
                if (finalBoss != null)
                {
                    StartCoroutine(finalBoss.DeathSequence());
                }
                else
                {
                    GameOver(true);
                }
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
                if (playerWon){
                    battleSFX?.PlayWinGameOver();
                    gameOverUI.ShowWin();
                } else {
                    gameOverUI.ShowDeath();
                    battleSFX?.PlayLoseGameOver();
                }
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

            // --- Decide where to spawn the enemy using enemySpawnPoint ---
            Vector3 pos = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            Quaternion rot = enemySpawnPoint != null ? enemySpawnPoint.rotation : Quaternion.identity;

            // If this enemy uses the Final Boss arena, lift the spawn a bit
            if (data.arenaBiome == ArenaBiome.FinalBoss)
            {
                pos.y = 2f;   // adjust this height until the orb looks right
            }

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
                attackCtrl.animator = enemyController.animator;
                attackCtrl.feedbackUI = feedbackUI;
                attackCtrl.battleSFX = FindObjectOfType<BattleSFX>();

                enemyAttackController = attackCtrl;
            }
            else
            {
                Debug.LogWarning("Spawned enemy has no EnemyAttackController.");
            }

            // Hook Name text
            if (enemyNameText != null)
                enemyNameText.text = data.displayName;
        }
    }
}
