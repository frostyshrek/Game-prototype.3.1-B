using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattleSystem;

[RequireComponent(typeof(Button))]
public class LibraryCardNameItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject lockedOverlay;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float smooth = 14f;

    private RectTransform rt;
    private Vector3 baseScale;

    private GladeLoadoutUI owner;
    private CardData card;
    private bool unlocked;
    private bool hovering;

    private Button btn;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();   // safer than "transform as RectTransform"
        btn = GetComponent<Button>();
        baseScale = rt != null ? rt.localScale : Vector3.one;
    }

    public void Bind(GladeLoadoutUI ui, CardData data, bool isUnlocked)
    {
        owner = ui;
        card = data;
        unlocked = isUnlocked;

        if (nameText != null)
            nameText.text = unlocked && card != null ? card.cardName : "?????";

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!unlocked);

        if (btn != null)
        {
            btn.interactable = unlocked;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (!unlocked || card == null) return;
                owner?.OnLibraryCardClicked(card);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!unlocked || card == null) return;
        hovering = true;
        owner?.OnHoverCard(card);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    private void Update()
    {
        if (rt == null) return;
        Vector3 target = hovering ? baseScale * hoverScale : baseScale;
        rt.localScale = Vector3.Lerp(rt.localScale, target, Time.deltaTime * smooth);
    }
}
