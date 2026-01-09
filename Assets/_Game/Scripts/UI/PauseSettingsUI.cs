using UnityEngine;
using UnityEngine.UI;

public class PauseSettingsUI : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject mainPage;
    [SerializeField] private GameObject settingsPage;

    // NEW: How To Play page
    [SerializeField] private GameObject howToPlayPage;

    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;

    [Header("Button Borders (child border objects/images)")]
    [SerializeField] private GameObject easyBorder;
    [SerializeField] private GameObject hardBorder;

    const string PREF_DIFFICULTY = "GS_Difficulty"; // 0 easy, 1 hard

    private void Awake()
    {
        // Hook buttons
        if (easyButton) easyButton.onClick.AddListener(SetEasy);
        if (hardButton) hardButton.onClick.AddListener(SetHard);

        // Ensure default difficulty exists
        if (!PlayerPrefs.HasKey(PREF_DIFFICULTY))
        {
            PlayerPrefs.SetInt(PREF_DIFFICULTY, 0);
            PlayerPrefs.Save();
        }

        RefreshDifficultyUI();

        // NEW: ensure only main page is visible at startup
        if (mainPage) mainPage.SetActive(true);
        if (settingsPage) settingsPage.SetActive(false);
        if (howToPlayPage) howToPlayPage.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshDifficultyUI();

        // optional safety (don’t show random pages if object re-enabled)
        // Comment these out if you *want* it to remember last page.
        if (mainPage) mainPage.SetActive(true);
        if (settingsPage) settingsPage.SetActive(false);
        if (howToPlayPage) howToPlayPage.SetActive(false);
    }

    // --- Page navigation ---
    public void OpenSettings()
    {
        if (mainPage) mainPage.SetActive(false);
        if (settingsPage) settingsPage.SetActive(true);
        if (howToPlayPage) howToPlayPage.SetActive(false);

        RefreshDifficultyUI();
    }

    public void CloseSettings()
    {
        if (settingsPage) settingsPage.SetActive(false);
        if (howToPlayPage) howToPlayPage.SetActive(false);
        if (mainPage) mainPage.SetActive(true);
    }

    // NEW: How To Play navigation
    public void OpenHowToPlay()
    {
        if (mainPage) mainPage.SetActive(false);
        if (settingsPage) settingsPage.SetActive(false);
        if (howToPlayPage) howToPlayPage.SetActive(true);
    }

    // Usually your Back button on HowToPlay should call this
    public void CloseHowToPlay()
    {
        if (howToPlayPage) howToPlayPage.SetActive(false);
        if (settingsPage) settingsPage.SetActive(false);
        if (mainPage) mainPage.SetActive(true);
    }

    // --- Difficulty ---
    public void SetEasy() => SetDifficulty(0);
    public void SetHard() => SetDifficulty(1);

    private void SetDifficulty(int value)
    {
        PlayerPrefs.SetInt(PREF_DIFFICULTY, value);
        PlayerPrefs.Save();
        RefreshDifficultyUI();
    }

    private void RefreshDifficultyUI()
    {
        bool hard = IsHardMode();

        if (easyBorder) easyBorder.SetActive(!hard);
        if (hardBorder) hardBorder.SetActive(hard);
    }

    public static bool IsHardMode()
    {
        return PlayerPrefs.GetInt(PREF_DIFFICULTY, 0) == 1;
    }
}
