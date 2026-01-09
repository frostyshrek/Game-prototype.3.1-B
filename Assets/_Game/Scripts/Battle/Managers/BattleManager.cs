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
        public EnemyAttackController enemyAttackController;   // spawned enemy's controller
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
                enemyAttackController.BeginAttacks();

            StartPlayerTurn();
        }

        void InitializeBattle()
        {
            cardManager.playerHand.Clear();
            cardManager.preparationArea.Clear();
            cardManager.discardPile.Clear();
            cardManager.drawPile.Clear();

            if (handUI == null)
                handUI = FindObjectOfType<HandUIController>(true);

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

            var ids = GameState.I.EquippedCardIds;
            if (ids == null || ids.Count == 0)
            {
                Debug.LogWarning("BattleManager: EquippedCardIds is empty. Did you save a deck in Glade?");
                return;
            }

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

        public void StartPlayerTurn()
        {
            isPlayerTurn = true;
            currentState = BattleState.PlayerTurn;
            cardsPlayedThisTurn = 0;

            effectResolver.ProcessDamageOverTimeEffects(EffectTarget.Self);

            if (playerMovement != null)
                playerMovement.SetCanMove(true);

            if (playerController.IsDead())
            {
                GameOver(false);
                return;
            }
        }

        public void EndPlayerTurn()
        {
            if (currentState != BattleState.PlayerTurn) return;

            if (cardManager.preparationArea == null || cardManager.preparationArea.Count == 0)
            {
                if (castButtonUI != null)
                    castButtonUI.FlashError();

                battleSFX?.PlayNotEffective();
                feedbackUI?.Show("NO CARDS SELECTED", FeedbackType.Error, 1.6f);
                return;
            }

            while (cardManager.IsHandOverLimit())
                cardManager.DiscardRandomHandCard();

            StartCoroutine(ResolveCardEffects());
        }

        IEnumerator ResolveCardEffects()
        {
            currentState = BattleState.ResolvingEffects;

            List<CardData> cardsToResolve = cardManager.GetPreparationCards();
            yield return StartCoroutine(effectResolver.ResolvePreparationEffects(cardsToResolve));

            cardManager.DiscardPreparationArea();

            if (enemyController.currentHealth <= 0)
            {
                var finalBoss = enemyController.GetComponent<FinalBossVisuals>();
                if (finalBoss != null) StartCoroutine(finalBoss.DeathSequence());
                else GameOver(true);
                yield break;
            }

            BuildRandomHandFromEquipped();
            StartPlayerTurn();
        }

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
                EnemyData data = GameState.I.CurrentEncounter;
                if (playerWon && data != null && data.arenaBiome == ArenaBiome.FinalBoss)
                {
                    var end = FindObjectOfType<FinalBossEndScreen>(true);
                    if (end != null) end.TriggerEnd();
                }

                if (data != null)
                {
                    // Card drop (MiniBoss + MainBoss)
                    if ((data.tier == EnemyTier.MiniBoss || data.tier == EnemyTier.MainBoss) && data.droppedCard != null)
                    {
                        GameState.I.UnlockCard(data.droppedCard);
                        feedbackUI?.Show($"NEW CARD: {data.droppedCard.cardName}", FeedbackType.Success, 2.0f);
                    }

                    // Rune drop (MainBoss only)
                    if (data.tier == EnemyTier.MainBoss)
                    {
                        GameState.I.GiveRune(data.droppedRune);
                        feedbackUI?.Show($"GREAT RUNE: {data.droppedRune}", FeedbackType.Effective, 2.4f);
                    }

                    GameState.I.SaveCards();
                    Debug.Log("[BattleManager] Forced SaveCards after rewards.");
                }

                // Encounter defeated tracking
                GameState.I.MarkEncounterDefeated(GameState.I.LastEncounterId);
            }

            if (gameOverUI != null)
            {
                if (playerWon)
                {
                    battleSFX?.PlayWinGameOver();
                    gameOverUI.ShowWin();
                }
                else
                {
                    battleSFX?.PlayLoseGameOver();
                    gameOverUI.ShowDeath();
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
                EndPlayerTurn();
        }

        void Update()
        {
            if (currentState == BattleState.PlayerTurn && Input.GetKeyDown(KeyCode.E))
                OnEndTurnButtonClicked();
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

            // final boss spawn height fix
            if (data.arenaBiome == ArenaBiome.FinalBoss)
                pos.y = 2f;

            GameObject enemyGO = Instantiate(data.battlePrefab, pos, rot);

            enemyController = enemyGO.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                Debug.LogError("Spawned enemy prefab has no EnemyController!");
                return;
            }

            enemyController.healthBarSlider = enemyHealthSlider;
            enemyController.healthText = enemyHealthText;
            enemyController.InitializeCharacter();

            if (effectResolver != null)
                effectResolver.enemyController = enemyController;

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
                attackCtrl.battleSFX = battleSFX;

                // MAP BIOME -> ATTRIBUTE and pass it once
                CardAttribute mapped = AttributeFromBiome(data.arenaBiome);
                attackCtrl.SetEnemyAttribute(mapped);

                enemyAttackController = attackCtrl;
            }
            else
            {
                Debug.LogWarning("Spawned enemy has no EnemyAttackController.");
            }

            if (enemyNameText != null)
                enemyNameText.text = data.displayName;
        }

        private CardAttribute AttributeFromBiome(ArenaBiome biome)
        {
            switch (biome)
            {
                case ArenaBiome.Fire:       return CardAttribute.Fire;
                case ArenaBiome.Ice:        return CardAttribute.Ice;
                case ArenaBiome.Forest:     return CardAttribute.Earth;
                case ArenaBiome.Lightning:  return CardAttribute.Lightning;
                case ArenaBiome.Castle:
                case ArenaBiome.FinalBoss:
                default:                    return CardAttribute.Physical;
            }
        }
    }
}
