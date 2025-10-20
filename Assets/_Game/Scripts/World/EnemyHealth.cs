using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 3;
    public GameObject keyDropPrefab;
    public Transform dropPoint;

    int hp;

    void Awake() => hp = maxHP;

    public void TakeDamage(int amount = 1)
    {
        if (hp <= 0) return;
        hp -= amount;
        if (hp <= 0) Die();
    }

    void Die()
    {
        // Spawn the key drop
        if (keyDropPrefab)
        {
            Vector3 pos = dropPoint ? dropPoint.position : transform.position + Vector3.up * 0.5f;
            Instantiate(keyDropPrefab, pos, Quaternion.identity);
        }

        // TODO: play death VFX/SFX here

        Destroy(gameObject);
    }
}
