using UnityEngine;

public enum Difficulty { Easy = 0, Hard = 1 }

public static class SettingsManager
{
    const string KEY_VOLUME = "SET_masterVolume";
    const string KEY_DIFF = "SET_difficulty";

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
        set
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_VOLUME, v);
            PlayerPrefs.Save();
            AudioListener.volume = v;
        }
    }

    public static Difficulty CurrentDifficulty
    {
        get => (Difficulty)PlayerPrefs.GetInt(KEY_DIFF, (int)Difficulty.Easy);
        set
        {
            PlayerPrefs.SetInt(KEY_DIFF, (int)value);
            PlayerPrefs.Save();
        }
    }
}
