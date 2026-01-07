using TMPro;
using UnityEngine;

public class BattleFeedbackUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform stackRoot;             // FeedbackStack
    [SerializeField] private FeedbackMessageUI messagePrefab; // FeedbackMessagePrefab

    [Header("Behaviour")]
    [SerializeField] private int maxMessages = 6;
    [SerializeField] private float defaultHold = 1.2f;

    // Simple colour choices (you can tweak)
    [Header("Colours")]
    [SerializeField] private Color info = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color success = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color warning = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField] private Color error = new Color(1f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color effective = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private Color notEffective = new Color(0.7f, 0.7f, 0.7f, 1f);

    public void Show(string msg, FeedbackType type, float duration = 1.4f)
    {
        if (stackRoot == null || messagePrefab == null)
        {
            Debug.LogWarning("[BattleFeedbackUI] Missing stackRoot or prefab");
            return;
        }

        var m = Instantiate(messagePrefab, stackRoot);

        m.SetColor(GetColor(type));

        // play message
        m.Play(msg, duration);

        m.transform.SetAsLastSibling();
    }

    private Color GetColor(FeedbackType type)
    {
        return type switch
        {
            FeedbackType.Success => success,
            FeedbackType.Warning => warning,
            FeedbackType.Error => error,
            FeedbackType.Effective => effective,
            FeedbackType.NotEffective => notEffective,
            _ => info,
        };
    }
}
