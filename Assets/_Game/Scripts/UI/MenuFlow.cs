using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFlow : MonoBehaviour
{
    [Header("Overlay")]
    public CanvasGroup blackOverlay;   // BlackOverlay 上的 CanvasGroup
    public TMP_Text storyText;         // StoryText

    [Header("Scene")]
    public string gameSceneName = "Glade";

    [Header("Timing")]
    public float fadeDuration = 1.0f;
    public float lineDelay = 0.9f;     // 每段之间停顿

    [TextArea(2, 6)]
    public string[] lines;             // 你的多段文本

    bool waitingForInput = false;

    void Awake()
    {
        // 初始状态：黑幕透明、不可挡输入
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            blackOverlay.blocksRaycasts = false;
            blackOverlay.interactable = false;
        }
        if (storyText != null) storyText.text = "";
    }

    void Update()
    {
        if (!waitingForInput) return;

        if (Input.anyKeyDown)
        {
            waitingForInput = false;
            SceneManager.LoadScene(gameSceneName);
        }
    }

    // 绑定到 New Game 按钮
    public void OnClickNewGame()
    {
        StopAllCoroutines();
        StartCoroutine(NewGameSequence());
    }

    IEnumerator NewGameSequence()
    {
        // 黑幕开始接管输入（防止玩家再点按钮）
        blackOverlay.blocksRaycasts = true;
        blackOverlay.interactable = true;

        // 1) 淡入到全黑
        yield return Fade(0f, 1f, fadeDuration);

        // 2) 显示你的文本（逐段追加）ADD OUR STORY HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        storyText.text = "TBC";
        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                storyText.text += lines[i] + "\n\n";
                yield return new WaitForSeconds(lineDelay);
            }
        }

        // 3) 等任意输入继续
        storyText.text += "\n<alpha=#AA>Press any key to continue";
        waitingForInput = true;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            blackOverlay.alpha = a;
            yield return null;
        }
        blackOverlay.alpha = to;
    }
}
