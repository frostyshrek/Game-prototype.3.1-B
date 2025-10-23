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
        public CharacterController characterController;

        [Header("Testing")]
        [SerializeField] private List<CardData> prototypeHand = new List<CardData>(); // put 3 cards here
        [SerializeField] private BattleSystem.HandUIController handUI;
        
        // [Header("Battle State")]
        public BattleState currentState = BattleState.PlayerTurn;
        // public int playerHealth = 100;
        // public int enemyHealth = 100;
        // public int maxHealth = 100;

        [Header("Turn Control")]
        public bool isPlayerTurn = true;
        public int cardsPlayedThisTurn = 0;

        // Battle States enum
        public enum BattleState
        {
            PlayerTurn,
            EnemyTurn,
            ResolvingEffects,
            GameOver
        }

        void Start()
        {
            InitializeBattle();
        }


        void InitializeBattle()
        {
            // Make sure CardManager is clean
            cardManager.playerHand.Clear();
            cardManager.preparationArea.Clear();
            cardManager.discardPile.Clear();
            cardManager.drawPile.Clear();

            // Put exactly the cards you want into hand
            cardManager.playerHand = new List<CardData>(prototypeHand);

            // Show them in UI
            var handUI = FindObjectOfType<BattleSystem.HandUIController>(true);
            if (handUI != null)
                handUI.RebuildHand(cardManager.playerHand);

            Debug.Log($"Prototype init: hand={cardManager.playerHand.Count} cards");
        }

        // initialize battle
        // void InitializeBattle()
        // {
        //     // using testing cards for now, will link to inventory
        //     List<CardData> testDeck = CreateTestDeck();
        //     cardManager.InitializeDeck(testDeck);
        //     cardManager.DrawStartingHand();

        //     var handUI = FindObjectOfType<BattleSystem.HandUIController>(true);
        //     if (handUI != null)
        //         handUI.RebuildHand(cardManager.playerHand);
            
        //     Debug.Log("Battle initialization finished, player hand card number: " + cardManager.playerHand.Count);
        // }

        // crate testing card deck (10 cards)
        List<CardData> CreateTestDeck()
        {
            // return empty for now, will change later
            return new List<CardData>();
        }

        // player's turn
        public void StartPlayerTurn()
        {
            isPlayerTurn = true;
            currentState = BattleState.PlayerTurn;
            effectResolver.ProcessDamageOverTimeEffects(EffectTarget.Self);
            cardsPlayedThisTurn = 0;

            if (playerController.IsDead()){
                GameOver(false);
                Debug.Log("Player hp below 0, lose");
            }
            
            Debug.Log("Player's turn starts");
        }

        // player's turn ends
        public void EndPlayerTurn()
        {
            // check hand card amount, discard randomly unitl 6
            while (cardManager.IsHandOverLimit())
            {
                cardManager.DiscardRandomHandCard();
            }
            
            Debug.Log("Player's turn ends, resolving card effects");
            StartCoroutine(ResolveCardEffects());
        }

        // resolving card effects
        IEnumerator ResolveCardEffects()
        {
            currentState = BattleState.ResolvingEffects;
            
            // get cards in preparation area to resolve effects
            List<CardData> cardsToResolve = cardManager.GetPreparationCards();
            Debug.Log($"resolving effects: {cardsToResolve.Count} cards");

            // resolving effects for cards (from left to right)
            yield return StartCoroutine(effectResolver.ResolvePreparationEffects(cardsToResolve));
    
            // discard cards in preparation area after resolving
            cardManager.DiscardPreparationArea();

            // check if game is over
            if (enemyController.currentHealth <= 0)
            {
                GameOver(true);
                yield break;
            }
    
            // start enemy's turn 
            StartEnemyTurn();
        }

        // enemy's turn
        void StartEnemyTurn()
        {
            currentState = BattleState.EnemyTurn;
            isPlayerTurn = false;
            
            Debug.Log("Enemy's turn starts");
            
            // simple enemy ai: deal 15 dmg to player
            playerController.TakeDamage(15);
            Debug.Log($"enemy attack! Player suffer 15 points of dmg, curret hp: {playerController.currentHealth}");
            
            // check if game is over
            if (playerController.currentHealth <= 0)
            {
                GameOver(false);
                return;
            }
            
            // end of enemy's turn 
            EndEnemyTurn();
        }

        // end of enemy's turn 
        // void EndEnemyTurn()
        // {
        //     // draw cards
        //     cardManager.DrawTurnCards();
        //     var handUI = FindObjectOfType<BattleSystem.HandUIController>(true);
        //     if (handUI != null)
        //         handUI.RebuildHand(cardManager.playerHand);
    
        //     Debug.Log($"draw card. {cardManager.GetDeckInfo()}");
    
        //     StartPlayerTurn();
        // }

        void EndEnemyTurn() 
        {
            // Prototype: always restore the same 3 cards
            cardManager.preparationArea.Clear();
            cardManager.playerHand = new List<CardData>(prototypeHand);
            cardManager.discardPile.Clear(); // optional, keeps it simple
            cardManager.drawPile.Clear();    // we’re not using draw pile

            var handUI = FindObjectOfType<BattleSystem.HandUIController>(true);
            if (handUI != null)
                handUI.RebuildHand(cardManager.playerHand);

            Debug.Log("Prototype refresh: restored fixed hand.");
            StartPlayerTurn();
        }

        // game over
        void GameOver(bool playerWon)
        {
            currentState = BattleState.GameOver;
            Debug.Log(playerWon ? "win!" : "lose!");

            if (playerWon && GameState.I != null)
            {
                // Mark the encounter that launched this battle as cleared
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

        // end turn button for this method
        public void OnEndTurnButtonClicked()
        {
            if (currentState == BattleState.PlayerTurn)
            {
                EndPlayerTurn();
            }
        }
    }
}