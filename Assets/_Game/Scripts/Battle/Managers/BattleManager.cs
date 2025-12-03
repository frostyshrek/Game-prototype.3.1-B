using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleSystem
{
    public class BattleManager : MonoBehaviour
    {
        public CardManager cardManager;
        public EffectResolver effectResolver;
        public PlayerController playerController;
        public EnemyController enemyController;

        [Header("New Battle System References")]
        public EnemyAttackController enemyAttackController;   // attach on Enemy
        public BattleOrbitMovement playerMovement;            // attach on Player
        public PlayerEnergy playerEnergy;                     // attach on Player

        [Header("Battle Loadout")]
        [Tooltip("5 cards the player has chosen before entering battle")]
        [SerializeField] private List<CardData> equippedBattleCards = new List<CardData>();  // set to 5 cards in Inspector
        [SerializeField] private int cardsPerHand = 3;

        [Header("UI")]
        [SerializeField] private HandUIController handUI;

        public BattleState currentState = BattleState.PlayerTurn;

        [Header("Turn Control")]
        public bool isPlayerTurn = true;
        public int cardsPlayedThisTurn = 0;

        public enum BattleState
        {
            PlayerTurn,
            ResolvingEffects,
            GameOver
        }

        void Start()
        {
            InitializeBattle();

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
            Debug.Log(playerWon ? "win!" : "lose!");

            if (playerMovement != null)
                playerMovement.SetCanMove(false);

            if (enemyAttackController != null)
                enemyAttackController.StopAttacks();   // stop continuous attacks

            if (playerWon && GameState.I != null)
            {
                GameState.I.MarkEncounterDefeated(GameState.I.LastEncounterId);
                GameState.I.GiveKey(KeyItem.AncientKey);
            }

            StartCoroutine(ReturnToGladeAfterDelay(1.5f));
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
    }
}
