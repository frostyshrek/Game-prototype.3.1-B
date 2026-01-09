using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using BattleSystem;

public class GladeLoadoutUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private List<CardData> starterCards = new List<CardData>(); // set 5 starter cards here

    [Header("Root (CanvasGroup on Panel_LoadoutRoot)")]
    [SerializeField] private CanvasGroup rootGroup;

    [Header("Top Slots (5)")]
    [SerializeField] private LoadoutSlotUI[] slots = new LoadoutSlotUI[5];

    [Header("Library (Name List)")]
    [SerializeField] private Transform libraryContent;                 // ScrollView Content
    [SerializeField] private LibraryCardNameItemUI libraryItemPrefab;  // Name item prefab

    [Header("Info Panel (Right Side)")]
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoDesc;
    [SerializeField] private TMP_Text infoCost;

    [Header("Error UI")]
    [SerializeField] private TMP_Text loadoutErrorText;
    [SerializeField] private float errorShowTime = 1.6f;

    [Header("Disable While Open")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable; // camera look + movement scripts

    [Header("Attribute Icon")]
    [SerializeField] private Image infoAttributeIcon;

    [Header("Attribute Sprites")]
    [SerializeField] private Sprite physicalIcon;
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite earthIcon;
    [SerializeField] private Sprite lightningIcon;

    private bool isOpen;
    private int selectedSlotIndex = 0;
    private Coroutine errorRoutine;

    private void Awake()
    {
        SetOpen(false, instant: true);
        ClearInfo();
        HideErrorInstant();
    }

    private void Start()
    {
        if (GameState.I != null)
        {
            GameState.I.EnsureStarterCards(starterCards);
            GameState.I.LoadCards();
        }
        Debug.Log("[Glade] unlocked = " + string.Join(", ", GameState.I.UnlockedCardIds));
        BuildNameLibrary();
        RefreshSlotsFromSave();
        SelectSlot(0);
        ClearInfo(); // don’t show template info
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Toggle();
    }

    // -------------------- OPEN / CLOSE --------------------

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        isOpen = true;
        SetOpen(true, instant: false);
        Time.timeScale = 0f;
        ClearInfo(); // don’t show anything until hover/click

        if (GameState.I != null)
        {
            GameState.I.LoadCards();
            Debug.Log("[Glade] unlocked = " + string.Join(", ", GameState.I.UnlockedCardIds));
            BuildNameLibrary();
        }
    }

    public void Close()
    {
        // Must have 5 cards equipped
        if (!HasFullDeck())
        {
            ShowLoadoutError("You must equip 5 cards in your deck.");
            return;
        }

        isOpen = false;
        Time.timeScale = 1f;
        SetOpen(false, instant: false);
        SaveEquipped();
    }

    private void SetOpen(bool open, bool instant)
    {
        if (rootGroup != null)
        {
            rootGroup.alpha = open ? 1f : 0f;
            rootGroup.interactable = open;
            rootGroup.blocksRaycasts = open;
        }

        // disable scripts while open
        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
                if (s) s.enabled = !open;
        }

        // cursor
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
    }

    // -------------------- LIBRARY (NAME LIST) --------------------

    private void BuildNameLibrary()
    {
        if (libraryContent == null || libraryItemPrefab == null || cardDatabase == null)
        {
            Debug.LogWarning("[GladeLoadoutUI] Missing libraryContent / prefab / database");
            return;
        }

        foreach (Transform child in libraryContent)
            Destroy(child.gameObject);

        foreach (var card in cardDatabase.allCards)
        {
            if (card == null) continue;

            bool unlocked = (GameState.I == null) || GameState.I.IsCardUnlocked(card);

            var item = Instantiate(libraryItemPrefab, libraryContent);
            item.Bind(this, card, unlocked);
        }
    }

    // called by LibraryCardNameItemUI hover
    public void OnHoverCard(CardData card)
    {
        ShowInfo(card);
    }

    // called by LibraryCardNameItemUI click
    public void OnLibraryCardClicked(CardData card)
    {
        if (card == null) return;

        if (GameState.I != null && !GameState.I.IsCardUnlocked(card))
            return;

        // prevent duplicates
        if (IsCardAlreadyEquipped(card))
        {
            ShowLoadoutError("That card is already in your deck.");
            return;
        }

        // Find first empty slot
        int targetSlot = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].Card == null)
            {
                targetSlot = i;
                break;
            }
        }

        // If none empty, overwrite selected slot
        if (targetSlot == -1)
            targetSlot = Mathf.Clamp(selectedSlotIndex, 0, slots.Length - 1);

        // Still prevent duplicates (overwrite case could overwrite with same card)
        if (slots[targetSlot] != null && slots[targetSlot].Card == card)
        {
            ShowLoadoutError("That card is already in your deck.");
            return;
        }

        slots[targetSlot].SetCard(this, targetSlot, card);
        SelectSlot(targetSlot);
        ShowInfo(card);
    }

    // -------------------- DECK SLOTS --------------------

    private void RefreshSlotsFromSave()
    {
        if (GameState.I == null || cardDatabase == null) return;

        var ids = GameState.I.EquippedCardIds;

        for (int i = 0; i < slots.Length; i++)
        {
            CardData c = (i < ids.Count) ? cardDatabase.FindById(ids[i]) : null;
            if (slots[i] != null)
                slots[i].SetCard(this, i, c);
        }
    }

    public void SelectSlot(int index)
    {
        selectedSlotIndex = Mathf.Clamp(index, 0, slots.Length - 1);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].SetSelected(i == selectedSlotIndex);
        }
    }

    public void OnHoverSlotCard(CardData card)
    {
        // Call this from LoadoutSlotUI when slot has a card and you hover it
        ShowInfo(card);
    }

    public void ClearDeck()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].SetCard(this, i, null);
        }

        SelectSlot(0);
        ClearInfo();
    }

    // -------------------- INFO PANEL --------------------

    private void ShowInfo(CardData card)
    {
        if (card == null) return;

        if (infoTitle) infoTitle.text = card.cardName;
        if (infoDesc) infoDesc.text = card.GetDescription();
        if (infoCost) infoCost.text = $"{card.energyCost}";
        if (infoAttributeIcon != null)
        {
            infoAttributeIcon.enabled = true;
            infoAttributeIcon.sprite = GetAttrIcon(card.baseAttribute);
            infoAttributeIcon.preserveAspect = true;
        }
    }

    private void ClearInfo()
    {
        if (infoTitle) infoTitle.text = "";
        if (infoDesc) infoDesc.text = "";
        if (infoCost) infoCost.text = "";
        if (infoAttributeIcon != null)
        {
            infoAttributeIcon.enabled = false;
        }
    }

    // -------------------- SAVE / VALIDATION --------------------

    private void SaveEquipped()
    {
        if (GameState.I == null) return;

        var list = new List<CardData>();
        for (int i = 0; i < slots.Length; i++)
            list.Add(slots[i] != null ? slots[i].Card : null);

        GameState.I.SetEquippedCards(list);
    }

    private bool HasFullDeck()
    {
        int count = 0;
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] != null && slots[i].Card != null) count++;

        return count >= 5;
    }

    private bool IsCardAlreadyEquipped(CardData card)
    {
        if (card == null) return false;

        for (int i = 0; i < slots.Length; i++)
            if (slots[i] != null && slots[i].Card == card) return true;

        return false;
    }

    // -------------------- ERROR --------------------

    private void ShowLoadoutError(string msg)
    {
        if (loadoutErrorText == null)
        {
            Debug.LogWarning(msg);
            return;
        }

        if (errorRoutine != null) StopCoroutine(errorRoutine);
        errorRoutine = StartCoroutine(ErrorRoutine(msg));
    }

    private IEnumerator ErrorRoutine(string msg)
    {
        loadoutErrorText.gameObject.SetActive(true);
        loadoutErrorText.text = msg;

        yield return new WaitForSeconds(errorShowTime);

        loadoutErrorText.gameObject.SetActive(false);
        errorRoutine = null;
    }

    private void HideErrorInstant()
    {
        if (loadoutErrorText != null)
            loadoutErrorText.gameObject.SetActive(false);
    }

    private Sprite GetAttrIcon(CardAttribute a)
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
