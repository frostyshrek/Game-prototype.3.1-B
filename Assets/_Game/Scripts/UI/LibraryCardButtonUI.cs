using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using BattleSystem;

[RequireComponent(typeof(RectTransform))]
public class LibraryCardButtonUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Refs")]
    [SerializeField] private CardView cardView;          // optional (if you want to reuse CardView visuals)
    [SerializeField] private GameObject lockedOverlay;   // optional overlay for locked cards

    [Header("Optional simple fields (if not using CardView)")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image attributeIconImage;

    [Header("Attribute Icons")]
    [SerializeField] private Sprite physicalIcon;
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite earthIcon;
    [SerializeField] private Sprite lightningIcon;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float smooth = 12f;

    private GladeLoadoutUI owner;
    private CardData card;
    private bool isUnlocked = true;

    private RectTransform rt;
    private Vector3 baseScale;
    private bool hovering;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
    }

    // GladeLoadoutUI calls this
    public void Bind(GladeLoadoutUI ui, CardData data)
    {
        owner = ui;
        card = data;

        isUnlocked = (GameState.I == null) || GameState.I.IsCardUnlocked(card);

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // If you’re reusing CardView visuals:
        if (cardView != null)
        {
            cardView.gameObject.SetActive(true);
            cardView.Bind(null, card); // controller null so it won't "play" cards
        }

        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        owner?.OnHoverCard(card);
    }

    public void OnPointerExit(PointerEventData eventData) => hovering = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUnlocked) return;
        owner?.OnLibraryCardClicked(card);
    }

    private void Update()
    {
        Vector3 target = hovering ? baseScale * hoverScale : baseScale;
        rt.localScale = Vector3.Lerp(rt.localScale, target, Time.deltaTime * smooth);
    }

    private void RefreshVisual()
    {
        if (card == null) return;

        if (!isUnlocked)
        {
            if (titleText) titleText.text = "";
            if (costText) costText.text = "";
            if (attributeIconImage) attributeIconImage.enabled = false;
            return;
        }

        if (titleText) titleText.text = card.cardName;
        if (costText) costText.text = card.energyCost.ToString();

        if (attributeIconImage)
        {
            attributeIconImage.enabled = true;
            attributeIconImage.sprite = GetIcon(card.baseAttribute);
        }
    }

    private Sprite GetIcon(CardAttribute a)
    {
        return a switch
        {
            CardAttribute.Fire => fireIcon,
            CardAttribute.Ice => iceIcon,
            CardAttribute.Earth => earthIcon,
            CardAttribute.Lightning => lightningIcon,
            _ => physicalIcon,
        };
    }
}
