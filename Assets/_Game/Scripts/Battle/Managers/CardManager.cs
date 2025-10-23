using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class CardManager : MonoBehaviour{
        [Header("card data")]
        public List<CardData> playerDeck = new List<CardData>();  // player deck
        public List<CardData> playerHand = new List<CardData>();  // player hand card
        public List<CardData> preparationArea = new List<CardData>(); // preparation area to hold cards during player's turn
        public List<CardData> discardPile = new List<CardData>(); // discard pile
        public List<CardData> drawPile = new List<CardData>();    // draw pile

        [Header("amount limit")]
        public int maxHandSize = 6;
        public int startingHandSize = 5;
        public int cardsPerTurn = 3; // Number of draws per turn
        public int maxCardsPerTurn = 3;
        public int maxPreparationSlots = 3;

        // initialze deck
        public void InitializeDeck(List<CardData> selectedDeck)
        {
            playerDeck = new List<CardData>(selectedDeck);
            drawPile = new List<CardData>(selectedDeck);
            playerHand.Clear();
            discardPile.Clear();

            ShuffleDrawPile();
        }

        // shuffle draw pile
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

        // draw cards
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
                    // if no cards to draw (which should never happen)
                    Debug.LogWarning("draw pile is empty, couldn't draw cards");
                    break;
                }
            }
        }

        // draw single card
        public CardData DrawSingleCard()
        {
            // check discard pile if draw pile is empty
            if (drawPile.Count == 0)
            {
                if (discardPile.Count > 0)
                {
                    // shuffle discard pile to draw pile 
                    drawPile = new List<CardData>(discardPile);
                    discardPile.Clear();
                    ShuffleDrawPile();
                    Debug.Log("shuffle discard pile to draw pile, current draw pile number: " + drawPile.Count);
                }
                else
                {
                    // no card to draw at all (which should never happen)
                    return null;
                }
            }

            // draw card
            CardData drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            return drawnCard;
        }

        // draw starting hand card
        public void DrawStartingHand()
        {
            DrawCards(startingHandSize);
            Debug.Log($"draw starting hand card, current hand card number: {playerHand.Count}");
        }

        // draw card at start turn
        public void DrawTurnCards()
        {
            DrawCards(cardsPerTurn);
            Debug.Log($"Round draw, current hand count: {playerHand.Count}");
        }

        // use card from hand
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
            playerHand.RemoveAt(handIndex);
            preparationArea.Add(card);

            OnPreparationChanged?.Invoke();
            return true;
        }

        // retrieve card from preparation area
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

        // swap cards in preparation area
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

        // discard preparation area cards to discard pile
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
            return preparationArea.IndexOf(card); // -1 if not found
        }

        // get preparation card list
        public List<CardData> GetPreparationCards()
        {
            return new List<CardData>(preparationArea);
        }   



        // check if player hand card amount is over limit
        public bool IsHandOverLimit()
        {
            return playerHand.Count > maxHandSize;
        }

        // discard random hand card until player holds only 6 cards
        public void DiscardRandomHandCard()
        {
            if (playerHand.Count > 0)
            {
                int randomIndex = Random.Range(0, playerHand.Count);
                CardData discardedCard = playerHand[randomIndex];
                playerHand.RemoveAt(randomIndex);
                DiscardCard(discardedCard);
                Debug.Log($"discard ramdom cards: {discardedCard.cardName}");
            }
        }

        public void DiscardCard(CardData card)
        {
            if (card != null)
            {
                discardPile.Add(card);
            }
        }

        // get deck info (for debug)
        public string GetDeckInfo()
        {
            return $"hand card number: {playerHand.Count}, draw pile number: {drawPile.Count}, discard pile number: {discardPile.Count}";
        }
    }
}