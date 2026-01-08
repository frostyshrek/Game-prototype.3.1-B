using System.Collections.Generic;
using UnityEngine;

public enum FeedbackTypeGlade
{
    Info,
    Success,
    Warning,
    Error
}

public class GladeFeedbackUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform stackRoot;                 // FeedbackStack
    [SerializeField] private GladeFeedbackMessageUI messagePrefab;

    [Header("Behaviour")]
    [SerializeField] private int maxMessages = 6;
    [SerializeField] private float defaultHold = 2.2f;
    [SerializeField] private float defaultFade = 0.35f;

    [Header("Colours")]
    [SerializeField] private Color info = Color.white;
    [SerializeField] private Color success = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color warning = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField] private Color error = new Color(1f, 0.4f, 0.4f, 1f);

    private readonly List<GladeFeedbackMessageUI> _alive = new();

    public void Show(string msg, FeedbackTypeGlade type = FeedbackTypeGlade.Info, float holdTime = -1f)
    {
        if (stackRoot == null || messagePrefab == null)
        {
            Debug.LogWarning("[GladeFeedbackUI] Missing stackRoot or messagePrefab");
            return;
        }

        float hold = (holdTime > 0f) ? holdTime : defaultHold;

        var m = Instantiate(messagePrefab, stackRoot);
        m.Play(msg, GetColor(type), hold, defaultFade);

        _alive.Add(m);
        Trim();
    }

    private void Trim()
    {
        // remove nulls
        for (int i = _alive.Count - 1; i >= 0; i--)
            if (_alive[i] == null) _alive.RemoveAt(i);

        // if too many, delete oldest (top)
        while (_alive.Count > maxMessages)
        {
            var oldest = _alive[0];
            _alive.RemoveAt(0);
            if (oldest != null) Destroy(oldest.gameObject);
        }
    }

    private Color GetColor(FeedbackTypeGlade type)
    {
        return type switch
        {
            FeedbackTypeGlade.Success => success,
            FeedbackTypeGlade.Warning => warning,
            FeedbackTypeGlade.Error => error,
            _ => info
        };
    }

    public static GladeFeedbackUI I { get; private set; }

    private void Awake()
    {
        I = this;
    }
}
