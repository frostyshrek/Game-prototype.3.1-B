using UnityEngine;
using BattleSystem;

public class ChestUnlockCard : MonoBehaviour, IInteractable
{
    [Header("Reward")]
    [SerializeField] private CardData cardToGive;

    [Header("UI / Feedback (optional)")]
    [SerializeField] private GladeFeedbackUI gladeFeedback;

    [Header("Chest State")]
    [Tooltip("Unique id for this chest. MUST be unique per chest in the whole game.")]
    [SerializeField] private string chestId = "chest_001";

    [Header("Visuals (optional)")]
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    const string PREF_OPEN_PREFIX = "CHEST_OPEN_";

    private bool opened;

    private void Awake()
    {
        // Auto-find feedback if not set
        if (gladeFeedback == null)
            gladeFeedback = FindObjectOfType<GladeFeedbackUI>(true);

        // Load opened state
        opened = PlayerPrefs.GetInt(PREF_OPEN_PREFIX + chestId, 0) == 1;
        ApplyVisualState();
    }

    public void Interact()
    {
        if (opened)
        {
            gladeFeedback?.Show("Chest is empty.", FeedbackTypeGlade.Info, 1.8f);
            return;
        }

        if (GameState.I == null)
        {
            gladeFeedback?.Show("No GameState found.", FeedbackTypeGlade.Error, 2.2f);
            return;
        }

        if (cardToGive == null)
        {
            gladeFeedback?.Show("Chest has no reward set.", FeedbackTypeGlade.Error, 2.2f);
            return;
        }

        // Unlock card
        GameState.I.UnlockCard(cardToGive);

        gladeFeedback?.Show($"FOUND CARD: {cardToGive.cardName}", FeedbackTypeGlade.Success, 2.2f);

        // Mark opened + save
        opened = true;
        PlayerPrefs.SetInt(PREF_OPEN_PREFIX + chestId, 1);
        PlayerPrefs.Save();

        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (closedVisual) closedVisual.SetActive(!opened);
        if (openVisual) openVisual.SetActive(opened);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(chestId))
            chestId = gameObject.name; // helps avoid empty ids
    }
#endif
}
