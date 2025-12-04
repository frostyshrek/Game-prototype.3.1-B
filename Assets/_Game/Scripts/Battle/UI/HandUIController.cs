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

        [SerializeField] private float cardSpacing = 80f;
        [SerializeField] private float angleStep   = 6f;
        [SerializeField] private float curveHeight = 8f;
        [SerializeField] private float baseY       = 40f;

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

            int count = hand.Count;
            if (count == 0) return;

            float baseY       = 40f;                        // lift cards off the very bottom
            float centerIndex = (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var data = hand[i];

                CardView view = Instantiate(cardViewPrefab, handContainer);
                view.name = $"CardView_{i}_{data.cardName}";
                view.Bind(this, data);
                handViews.Add(view);

                RectTransform rt = (RectTransform)view.transform;

                float offsetIndex = i - centerIndex;

                float angle = offsetIndex * angleStep;
                float x = -offsetIndex * cardSpacing;
                float y = baseY + Mathf.Abs(offsetIndex) * curveHeight;

                rt.anchoredPosition = new Vector2(x, y);
                rt.localRotation    = Quaternion.Euler(0, 0, angle);
                rt.localScale       = Vector3.one;
                rt.SetAsLastSibling();

                view.SetBasePose(rt.anchoredPosition, rt.localScale);
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
