using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class CardManager : MonoBehaviour
    {
        [Header("card data")]
        public List<CardData> playerDeck = new List<CardData>();      // player deck
        public List<CardData> playerHand = new List<CardData>();      // player hand
        public List<CardData> preparationArea = new List<CardData>(); // cards chosen this turn
        public List<CardData> discardPile = new List<CardData>();     // discard pile
        public List<CardData> drawPile = new List<CardData>();        // draw pile

        [Header("amount limit")]
        public int maxHandSize = 6;
        public int startingHandSize = 5;
        public int cardsPerTurn = 3;
        public int maxCardsPerTurn = 3;
        public int maxPreparationSlots = 3;

        [Header("UI")]
        public BattleFeedbackUI feedbackUI;

        [Header("Energy")]
        public PlayerEnergy playerEnergy;
        public EnergyBarFeedback energyBarFeedback;

        public BattleSFX battleSFX;

        // ---- deck init / draw (unchanged) ----

        public void InitializeDeck(List<CardData> selectedDeck)
        {
            playerDeck = new List<CardData>(selectedDeck);
            drawPile = new List<CardData>(selectedDeck);
            playerHand.Clear();
            discardPile.Clear();

            ShuffleDrawPile();
        }

        public void ShuffleDrawPile()
        {
            for (int i = 0; i < drawPile.Count; i++)
            {
                CardData temp = drawPile[i];
                int randomIndex = Random.Range(i, drawPile.Count);
                drawPile[i] = drawPile[randomIndex];
                drawPile[randomIndex] = temp;
            }
        }

        public void DrawCards(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                CardData drawnCard = DrawSingleCard();
                if (drawnCard != null)
                {
                    playerHand.Add(drawnCard);
                }
                else
                {
                    Debug.LogWarning("draw pile is empty, couldn't draw cards");
                    break;
                }
            }
        }

        public CardData DrawSingleCard()
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count > 0)
                {
                    drawPile = new List<CardData>(discardPile);
                    discardPile.Clear();
                    ShuffleDrawPile();
                    Debug.Log("shuffle discard pile to draw pile, current draw pile number: " + drawPile.Count);
                }
                else
                {
                    return null;
                }
            }

            CardData drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            return drawnCard;
        }

        public void DrawStartingHand()
        {
            DrawCards(startingHandSize);
            Debug.Log($"draw starting hand card, current hand card number: {playerHand.Count}");
        }

        public void DrawTurnCards()
        {
            DrawCards(cardsPerTurn);
            Debug.Log($"Round draw, current hand count: {playerHand.Count}");
        }

        // ---- playing cards / preparation area ----

        public bool PlaceCardToPreparation(int handIndex)
        {
            if (handIndex < 0 || handIndex >= playerHand.Count)
            {
                Debug.LogWarning("preparation index invalid: PlaceCardToPreparation");
                return false;
            }
            if (preparationArea.Count >= maxPreparationSlots)
            {
                Debug.LogWarning("preparation area full");
                return false;
            }

            CardData card = playerHand[handIndex];

            // spend energy when card is placed into the combo
            if (playerEnergy != null && card.energyCost > 0)
            {
                if (!playerEnergy.TrySpend(card.energyCost))
                {
                    Debug.Log($"Not enough energy to play card: {card.cardName}");

                    battleSFX?.PlayNotEffective();

                    if (energyBarFeedback != null)
                        energyBarFeedback.PlayNotEnoughEnergyFeedback();

                    if (feedbackUI != null)
                        feedbackUI.Show("NOT ENOUGH ENERGY", FeedbackType.Error, 1.2f);

                    return false;
                }
            }

            playerHand.RemoveAt(handIndex);
            preparationArea.Add(card);

            OnPreparationChanged?.Invoke();
            return true;
        }

        public bool RetrieveCardFromPreparation(int preparationIndex)
        {
            if (preparationIndex < 0 || preparationIndex >= preparationArea.Count)
            {
                Debug.LogWarning("preparation index invalid: retrieve");
                return false;
            }

            CardData card = preparationArea[preparationIndex];
            preparationArea.RemoveAt(preparationIndex);
            playerHand.Add(card);

            Debug.Log($"card taken from preparation area: {card.cardName}, number of cards in preparation area: {preparationArea.Count}/{maxPreparationSlots}");
            
            OnPreparationChanged?.Invoke();
            return true;
        }

        public void SwapPreparationCards(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= preparationArea.Count ||
                indexB < 0 || indexB >= preparationArea.Count)
            {
                Debug.LogWarning("preparation index invalid: swap");
                return;
            }

            CardData temp = preparationArea[indexA];
            preparationArea[indexA] = preparationArea[indexB];
            preparationArea[indexB] = temp;

            Debug.Log($"Preparation area card swap: {indexA} <-> {indexB}");

            OnPreparationChanged?.Invoke();
        }

        public void DiscardPreparationArea()
        {
            if (preparationArea.Count > 0)
            {
                discardPile.AddRange(preparationArea);
                Debug.Log($"preparation area cards discard: {preparationArea.Count} cards");
                preparationArea.Clear();
            }

            OnPreparationChanged?.Invoke();
        }

        public event System.Action OnPreparationChanged;

        public int GetPreparationIndex(CardData card)
        {
            return preparationArea.IndexOf(card);
        }

        public List<CardData> GetPreparationCards()
        {
            return new List<CardData>(preparationArea);
        }   

        // ---- hand limit / discard ----

        public bool IsHandOverLimit()
        {
            return playerHand.Count > maxHandSize;
        }

        public void DiscardRandomHandCard()
        {
            if (playerHand.Count > 0)
            {
                int randomIndex = Random.Range(0, playerHand.Count);
                CardData discardedCard = playerHand[randomIndex];
                playerHand.RemoveAt(randomIndex);
                DiscardCard(discardedCard);
                Debug.Log($"discard random card: {discardedCard.cardName}");
            }
        }

        public void DiscardCard(CardData card)
        {
            if (card != null)
            {
                discardPile.Add(card);
            }
        }

        public string GetDeckInfo()
        {
            return $"hand card number: {playerHand.Count}, draw pile number: {drawPile.Count}, discard pile number: {discardPile.Count}";
        }
    }
}
