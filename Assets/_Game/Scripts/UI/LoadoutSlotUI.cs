using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattleSystem;

public class LoadoutSlotUI : MonoBehaviour, IPointerEnterHandler
{
    [Header("Refs")]
    [SerializeField] private Button button;

    [Header("Visuals")]
    [SerializeField] private GameObject emptyFrame;     // shown when Card == null
    [SerializeField] private GameObject filledRoot;     // shown when Card != null (contains your card UI)
    [SerializeField] private Image selectedHighlight;   // optional glow/outline

    [Header("Optional: show name/cost on slot")]
    [SerializeField] private TMPro.TMP_Text titleText;
    [SerializeField] private TMPro.TMP_Text costText;

    public CardData Card { get; private set; }

    private GladeLoadoutUI owner;
    private int index;

    public void SetCard(GladeLoadoutUI ui, int slotIndex, CardData card)
    {
        owner = ui;
        index = slotIndex;
        Card = card;

        // visuals
        bool hasCard = (Card != null);

        if (emptyFrame) emptyFrame.SetActive(!hasCard);
        if (filledRoot) filledRoot.SetActive(hasCard);

        if (titleText) titleText.text = hasCard ? Card.cardName : "";
        if (costText)  costText.text  = hasCard ? Card.energyCost.ToString() : "";

        // button click -> select this slot
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => owner.SelectSlot(index));
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
            selectedHighlight.enabled = selected;
    }

    // Hover on deck slots should show info too
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Card != null && owner != null)
            owner.OnHoverSlotCard(Card);
    }
}
