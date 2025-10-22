using UnityEngine;

/// 把这个脚本挂在“世界空间血条的根物体”（即血条的 World Space Canvas 上）
/// 作用：让血条始终面向摄像机，避免从侧面看时“变成一条线”
/// 用法：直接挂上即可；如果只想绕Y轴旋转（保持上下不歪），就把 onlyRotateAroundY 打勾。
public class BillboardUI : MonoBehaviour
{
    [Tooltip("是否只绕Y轴朝向相机（保持上下不歪）。不开启则完整朝向相机。")]
    public bool onlyRotateAroundY = true;

    [Tooltip("指定摄像机。不指定则默认使用 Camera.main")]
    public Camera targetCamera;

    void Reset()
    {
        // 默认尝试填充主摄像机
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            // 尝试动态获取主摄像机（兼容运行时切换）
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        if (onlyRotateAroundY)
        {
            // 只围绕世界Y轴转向相机：血条始终竖直，不会“歪头”
            Vector3 camPos = targetCamera.transform.position;
            Vector3 lookPos = new Vector3(camPos.x, transform.position.y, camPos.z);
            transform.LookAt(lookPos);
            // 让正面朝向相机（LookAt 默认让Z轴指向目标，世界空间UI正面通常是 -Z 朝向）
            transform.Rotate(0f, 180f, 0f); 
        }
        else
        {
            // 完整对准相机（包括俯仰）：视觉上更精确
            transform.LookAt(targetCamera.transform);
            transform.Rotate(0f, 180f, 0f);
        }
    }
}
