using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class HandUIController : MonoBehaviour
    {
        [Header("Refs")]
        public CardManager cardManager;
        public Transform handContainer; // parent with CardView instances

        [Header("Prefabs")]
        [SerializeField] private CardView cardViewPrefab;

        // Track the views (index corresponds to hand index)
        private readonly List<CardView> handViews = new();

        private void OnEnable()
        {
            if (cardManager != null)
                cardManager.OnPreparationChanged += RefreshOrderBadges;
        }

        private void OnDisable()
        {
            if (cardManager != null)
                cardManager.OnPreparationChanged -= RefreshOrderBadges;
        }

        // Call this whenever you redraw the hand (after draws/discards)
        public void RebuildHand(List<CardData> hand)
        {
            // clear old
            foreach (Transform child in handContainer) Destroy(child.gameObject);
            handViews.Clear();

            // build new
            for (int i = 0; i < hand.Count; i++)
            {
                var data = hand[i];

                // instantiate prefab under the container
                CardView view = Instantiate(cardViewPrefab, handContainer);
                view.name = $"CardView_{i}_{data.cardName}";
                view.Bind(this, data);

                handViews.Add(view);
            }

            RefreshOrderBadges();
        }

        public void OnCardClicked(CardView view)
        {
            // find current hand index
            int handIndex = handViews.IndexOf(view);
            if (handIndex < 0)
            {
                Debug.LogWarning("Clicked card not in current hand list.");
                return;
            }

            // ask CardManager to move it into preparation (enforces limits)
            bool placed = cardManager.PlaceCardToPreparation(handIndex);
            if (!placed) return;

            // After moving, hand list changed: remove that view from handViews
            handViews.RemoveAt(handIndex);

            // You may want to visually move the card to a Preparation UI row — left to right.
            // For now, just update badges in-place:
            RefreshOrderBadges();
        }

        private void RefreshOrderBadges()
        {
            // Clear all first
            foreach (var v in handViews) v.ClearSelectionVisuals();

            // Mark the ones that are in preparation with their order
            var prep = cardManager.GetPreparationCards();
            for (int i = 0; i < prep.Count; i++)
            {
                // Find any CardView still showing that CardData (if it stayed in hand UI it won’t be found)
                var view = handViews.Find(v => v.cardData == prep[i]);
                if (view != null)
                {
                    view.ApplySelectedVisuals(i + 1);
                }
            }
        }
    }
}
