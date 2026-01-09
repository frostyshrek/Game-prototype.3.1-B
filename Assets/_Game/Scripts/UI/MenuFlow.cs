using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFlow : MonoBehaviour
{
    [Header("Overlay")]
    public CanvasGroup blackOverlay;   // BlackOverlay �ϵ� CanvasGroup
    public TMP_Text storyText;         // StoryText

    [Header("Scene")]
    public string gameSceneName = "Glade";

    [Header("Timing")]
    public float fadeDuration = 1.0f;
    public float lineDelay = 0.9f;     // ÿ��֮��ͣ��

    [TextArea(2, 6)]
    public string[] lines;             // ��Ķ���ı�

    [TextArea(10, 40)]
    public string fullStory;

    bool waitingForInput = false;

    void Awake()
    {
        // ��ʼ״̬����Ļ͸�������ɵ�����
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

    // �󶨵� New Game ��ť
    public void OnClickNewGame()
    {
        StopAllCoroutines();
        StartCoroutine(NewGameSequence());
    }

    IEnumerator NewGameSequence()
    {
        // ��Ļ��ʼ�ӹ����루��ֹ����ٵ㰴ť��
        blackOverlay.blocksRaycasts = true;
        blackOverlay.interactable = true;

        // 1) ���뵽ȫ��
        yield return Fade(0f, 1f, fadeDuration);

        // 2) ��ʾ����ı������׷�ӣ�ADD OUR STORY HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        storyText.text = fullStory;          // fullStory ��������γ������ַ���
        storyText.pageToDisplay = 1;         // �ӵ�1ҳ��ʼ

        // ǿ�� TMP �ȼ����ҳ��Ϣ�����룩
        storyText.ForceMeshUpdate();

        int totalPages = storyText.textInfo.pageCount;

        while (true)
        {
            // �ȴ��������루����/��꣩
            yield return new WaitUntil(() => Input.anyKeyDown);

            // ��ֹһ֡�ڶ�δ���
            yield return null;

            if (storyText.pageToDisplay < totalPages)
            {
                storyText.pageToDisplay++;
            }
            else
            {
                break; // ���һҳ���º������ҳ������ִ�к���ġ�����Ϸ���߼�
            }
        }
        storyText.text += "\n\n<color=#FFFFFFAA>Press any key to continue</color>";

        // �����һ������
        yield return new WaitUntil(() => Input.anyKeyDown);
        yield return null;

        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Glade");

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
}
