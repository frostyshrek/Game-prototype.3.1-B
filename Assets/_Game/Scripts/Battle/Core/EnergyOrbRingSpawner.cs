using UnityEngine;

public class EnergyOrbRingSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform enemyCenter;
    public BattleOrbitMovement playerMovement;
    public GameObject orbPrefab;

    [Header("Ring Settings")]
    public int orbCount = 6;
    public float heightOffset = 0.5f;

    private void Start()
    {
        if (enemyCenter == null || playerMovement == null || orbPrefab == null)
        {
            Debug.LogWarning("EnergyOrbRingSpawner: missing refs");
            return;
        }

        float radius = playerMovement.OrbitRadius;

        for (int i = 0; i < orbCount; i++)
        {
            float angleDeg = (360f / orbCount) * i;
            float rad = angleDeg * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * radius;
            Vector3 pos = enemyCenter.position + offset + new Vector3(0f, heightOffset, 0f);

            Instantiate(orbPrefab, pos, Quaternion.identity, transform);
        }
    }
}
