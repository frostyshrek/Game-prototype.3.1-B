using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace BattleSystem
{
    public class CardView : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        [Header("UI Refs")]
        public TMP_Text titleText;          // "Card title bar" -> text
        public TMP_Text descriptionText;    // "Card description" -> text
        public Image artImage;              // "Card Image" -> Image (optional)
        public TMP_Text costText;           // "Card cost" -> text (optional)

        [Header("Selection Visuals")]
        public Image highlightFrame;
        public GameObject orderBadgeRoot;
        public TMP_Text orderBadgeText;

        [Header("Data (runtime)")]
        public CardData cardData;
        public bool isSelected;
        public int resolveOrder;

        private HandUIController controller;

        // Called by HandUIController after Instantiate
        public void Bind(HandUIController owner, CardData data)
        {
            controller = owner;
            cardData = data;
            Refresh();
            ClearSelectionVisuals();
        }

        public void Refresh()
        {
            if (cardData == null) return;
            if (titleText)       titleText.text = string.IsNullOrEmpty(cardData.cardName) ? "Card" : cardData.cardName;
            if (descriptionText) descriptionText.text = cardData.GetDescription();

            // if (artImage) artImage.sprite = cardData.cardImage;

            // if (costText) costText.text = cardData.cost.ToString();
        }

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData e)
        {
            if (controller == null) return;
            controller.OnCardClicked(this);
        }

        public void ApplySelectedVisuals(int order)
        {
            isSelected = true;
            resolveOrder = order;
            if (highlightFrame)  highlightFrame.enabled = true;
            if (orderBadgeRoot)  orderBadgeRoot.SetActive(true);
            if (orderBadgeText)  orderBadgeText.text = order.ToString();
        }

        public void ClearSelectionVisuals()
        {
            isSelected = false;
            resolveOrder = 0;
            if (highlightFrame)  highlightFrame.enabled = false;
            if (orderBadgeRoot)  orderBadgeRoot.SetActive(false);
            if (orderBadgeText)  orderBadgeText.text = "";
        }
    }
}
