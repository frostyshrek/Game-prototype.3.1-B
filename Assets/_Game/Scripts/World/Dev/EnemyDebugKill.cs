using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDebugKill : MonoBehaviour
{
    public KeyCode killKey = KeyCode.K;

    void Update()
    {
        if (Input.GetKeyDown(killKey))
        {
            GetComponent<EnemyHealth>().TakeDamage(999);
        }
    }
}
