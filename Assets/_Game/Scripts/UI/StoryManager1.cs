using UnityEngine;
using TMPro; // 使用 TextMeshPro 必加
using System.Collections; // 使用协程等待功能必加

public class StoryManager1 : MonoBehaviour
{
    [Header("UI 引用设置")]
    public TextMeshProUGUI storyText1; // 拖入层级面板中的 Text_Story
    public Animator animator1;         // 拖入层级面板中的 Text_Story (它带有动画组件)

    [Header("剧情文字内容")]
    [TextArea(3, 10)] // 让输入框在面板里大一点，方便填台词
    public string[] stories1;

    private int index1 = 0;            // 当前播放到第几行
    private bool isTransitioning1 = false; // 防止玩家点太快导致动画错乱

    // 第一步：当点击黑色屏幕时，调用这个函数
    public void OnClickStoryNext1()
    {
        // 如果正在切换中，直接跳过，不理会点击
        if (isTransitioning1) return;

        // 检查是否还有下一行文字
        if (index1 < stories1.Length - 1)
        {
            index1++;
            // 开启“等待协程”，实现先消失再出现的逻辑
            StartCoroutine(PageTransitionCoroutine1());
        }
        else
        {
            Debug.Log("所有剧情已播放完毕");
            // 以后可以在这里写跳转关卡的代码：SceneManager.LoadScene(1);
        }
    }

    // 第二步：核心过渡逻辑
    IEnumerator PageTransitionCoroutine1()
    {
        isTransitioning1 = true;

        // 1. 播放“消失”动画 (名字必须和你在 Animation 窗口录制的一模一样)
        animator1.Play("FadeOut1");

        // 2. 等待动画播完 (我们在录制时设定的时长是 0:30，所以这里等 0.5 秒)
        yield return new WaitForSeconds(0.5f);

        // 3. 在完全黑屏的状态下，更换文字内容
        storyText1.text = stories1[index1];

        // 4. 播放“出现”动画
        animator1.Play("FadeIn1");

        isTransitioning1 = false;
    }
}