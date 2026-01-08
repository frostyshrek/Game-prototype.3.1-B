using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BattleSystem;

public enum KeyItem
{
    AncientKey = 0,
}

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }

    // --- Inventory ---
    HashSet<KeyItem> _keys = new HashSet<KeyItem>();
    public event Action<KeyItem> OnKeyAdded;

    // --- Checkpoint ---
    public Vector3 CheckpointPos { get; private set; }
    public Quaternion CheckpointRot { get; private set; }
    public string CheckpointScene { get; private set; }  // optional: ensure same scene

    // ---- Current encounter data for Battle scene ----
    public EnemyData CurrentEncounter { get; private set; }

    [SerializeField] private List<string> unlockedCardIds = new List<string>();
    [SerializeField] private List<string> equippedCardIds = new List<string>(); // must be 5

    public IReadOnlyList<string> UnlockedCardIds => unlockedCardIds;
    public IReadOnlyList<string> EquippedCardIds => equippedCardIds;

    public void SetCurrentEncounter(EnemyData data)
    {
        CurrentEncounter = data;
    }

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // -------- Inventory API --------
    public bool HasKey(KeyItem key) => _keys.Contains(key);

    public void GiveKey(KeyItem key)
    {
        if (_keys.Add(key))
            OnKeyAdded?.Invoke(key);
    }

    public void RemoveKey(KeyItem key) => _keys.Remove(key);

    // -------- Checkpoint API --------
    public void SetCheckpoint(Transform t)
    {
        CheckpointPos = t.position;
        CheckpointRot = t.rotation;
        CheckpointScene = SceneManager.GetActiveScene().name;
        // Debug.Log($"Checkpoint set @ {CheckpointPos} in {CheckpointScene}");
    }

    // -------- Optional quick save/load (prototype) --------
    const string PFX = "GS_";
    public void QuickSave()
    {
        PlayerPrefs.SetFloat(PFX+"px", CheckpointPos.x);
        PlayerPrefs.SetFloat(PFX+"py", CheckpointPos.y);
        PlayerPrefs.SetFloat(PFX+"pz", CheckpointPos.z);
        PlayerPrefs.SetFloat(PFX+"rx", CheckpointRot.eulerAngles.x);
        PlayerPrefs.SetFloat(PFX+"ry", CheckpointRot.eulerAngles.y);
        PlayerPrefs.SetFloat(PFX+"rz", CheckpointRot.eulerAngles.z);
        PlayerPrefs.SetString(PFX+"scene", CheckpointScene);

        // Save keys as comma string
        PlayerPrefs.SetString(PFX+"keys", string.Join(",", _keys));
        PlayerPrefs.Save();
    }

    public void QuickLoad()
    {
        _keys.Clear();
        string keyStr = PlayerPrefs.GetString(PFX+"keys", "");
        if (!string.IsNullOrEmpty(keyStr))
        {
            foreach (var s in keyStr.Split(','))
                if (Enum.TryParse(s, out KeyItem k)) _keys.Add(k);
        }

        var scene = PlayerPrefs.GetString(PFX+"scene", "");
        CheckpointScene = scene;
        CheckpointPos = new Vector3(
            PlayerPrefs.GetFloat(PFX+"px", 0),
            PlayerPrefs.GetFloat(PFX+"py", 0),
            PlayerPrefs.GetFloat(PFX+"pz", 0));
        var r = new Vector3(
            PlayerPrefs.GetFloat(PFX+"rx", 0),
            PlayerPrefs.GetFloat(PFX+"ry", 0),
            PlayerPrefs.GetFloat(PFX+"rz", 0));
        CheckpointRot = Quaternion.Euler(r);
    }

    // ---- Encounter tracking ----
    public string LastEncounterId { get; private set; } = null;
    HashSet<string> _defeatedEncounters = new HashSet<string>();

    public void SetLastEncounterId(string id) => LastEncounterId = id;

    public void MarkEncounterDefeated(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        _defeatedEncounters.Add(id);
        PlayerPrefs.SetString("GS_defeated", string.Join(",", _defeatedEncounters));
        PlayerPrefs.Save();
    }

    public bool IsEncounterDefeated(string id)
    {
        return !string.IsNullOrEmpty(id) && _defeatedEncounters.Contains(id);
    }

    // load defeated set when booting
    void Start()
    {
        var saved = PlayerPrefs.GetString("GS_defeated", "");
        if (!string.IsNullOrEmpty(saved))
        {
            _defeatedEncounters.Clear();
            foreach (var s in saved.Split(','))
                if (!string.IsNullOrEmpty(s)) _defeatedEncounters.Add(s);
        }
    }

    public void EnsureStarterCards(List<CardData> starterCards)
    {
        if (starterCards == null) return;

        // If player has nothing unlocked yet, give starters once.
        LoadCards();
        if (unlockedCardIds.Count > 0) return;

        foreach (var c in starterCards)
            if (c != null && !unlockedCardIds.Contains(c.cardId))
                unlockedCardIds.Add(c.cardId);

        // Auto-equip first 5
        equippedCardIds.Clear();
        for (int i = 0; i < 5 && i < starterCards.Count; i++)
            if (starterCards[i] != null)
                equippedCardIds.Add(starterCards[i].cardId);

        SaveCards();
    }

    public bool IsCardUnlocked(CardData c)
    {
        if (c == null) return false;
        return unlockedCardIds.Contains(c.cardId);
    }

    public void UnlockCard(CardData c)
    {
        if (c == null) return;
        if (!unlockedCardIds.Contains(c.cardId))
        {
            unlockedCardIds.Add(c.cardId);
            SaveCards();
        }
    }

    public void SetEquippedCards(List<CardData> cards)
    {
        equippedCardIds.Clear();
        if (cards != null)
        {
            for (int i = 0; i < 5 && i < cards.Count; i++)
                if (cards[i] != null)
                    equippedCardIds.Add(cards[i].cardId);
        }
        SaveCards();
    }

    const string CARD_UNLOCK_KEY = "GS_unlocked_cards";
    const string CARD_EQUIP_KEY  = "GS_equipped_cards";

    public void SaveCards()
    {
        PlayerPrefs.SetString(CARD_UNLOCK_KEY, string.Join(",", unlockedCardIds));
        PlayerPrefs.SetString(CARD_EQUIP_KEY, string.Join(",", equippedCardIds));
        PlayerPrefs.Save();
    }

    public void LoadCards()
    {
        unlockedCardIds.Clear();
        equippedCardIds.Clear();

        var u = PlayerPrefs.GetString(CARD_UNLOCK_KEY, "");
        if (!string.IsNullOrEmpty(u))
            unlockedCardIds.AddRange(u.Split(','));

        var e = PlayerPrefs.GetString(CARD_EQUIP_KEY, "");
        if (!string.IsNullOrEmpty(e))
            equippedCardIds.AddRange(e.Split(','));
    }
}
