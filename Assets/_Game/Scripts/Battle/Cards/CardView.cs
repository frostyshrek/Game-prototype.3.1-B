using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace BattleSystem
{
    public class CardView : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
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

        [Header("Hover")]
        [SerializeField] private float hoverLift = 55f;
        [SerializeField] private float hoverScale = 1.2f;
        [SerializeField] private float hoverSmooth = 12f;

        private HandUIController controller;

        private RectTransform rect;
        private Vector2 baseAnchoredPos;
        private Vector3 baseScale = Vector3.one;
        private bool hasBasePose = false;
        private bool isHovering = false;

        // Called by HandUIController after Instantiate
        public void Bind(HandUIController owner, CardData data)
        {
            controller = owner;
            cardData = data;
            rect = GetComponent<RectTransform>();
            Refresh();
            ClearSelectionVisuals();
        }

        // NEW: only store the base pose, don't move the rect here
        public void SetBasePose(Vector2 anchoredPos, Vector3 scale)
        {
            if (rect == null)
                rect = GetComponent<RectTransform>();

            baseAnchoredPos = anchoredPos;
            baseScale = scale;
            hasBasePose = true;
        }

        public void Refresh()
        {
            if (cardData == null) return;

            if (titleText)
                titleText.text = string.IsNullOrEmpty(cardData.cardName)
                    ? "Card"
                    : cardData.cardName;

            // For hand view, hide the long description for now
            if (descriptionText)
            {
                descriptionText.text = "";
                // descriptionText.gameObject.SetActive(false); // if you prefer hidden
            }

            // if (artImage) artImage.sprite = cardData.cardImage;

            // Cost in corner – uses the CardData.energyCost
            if (costText)
                costText.text = cardData.energyCost.ToString();
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

        public void OnPointerClick(PointerEventData e)
        {
            if (controller == null) return;
            controller.OnCardClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
        }

        private void Update()
        {
            if (!hasBasePose || rect == null) return;

            // pick target position + scale
            Vector2 targetPos = baseAnchoredPos;
            Vector3 targetScale = baseScale;

            if (isHovering)
            {
                targetPos += new Vector2(0f, hoverLift);
                targetScale = baseScale * hoverScale;
            }

            // smooth movement & scale (also used for slide-in)
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * hoverSmooth);
            rect.localScale       = Vector3.Lerp(rect.localScale,       targetScale, Time.deltaTime * hoverSmooth);
        }

        public void BindForMenu(CardData data)
        {
            controller = null;
            cardData = data;
            Refresh();
            ClearSelectionVisuals();
        }
    }
}
