using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // Optional: persist
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
}
