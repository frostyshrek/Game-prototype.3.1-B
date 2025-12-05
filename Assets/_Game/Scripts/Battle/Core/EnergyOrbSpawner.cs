using UnityEngine;
using System.Collections;

public class EnergyOrbSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform enemyCenter;
    public BattleOrbitMovement playerMovement;
    public GameObject orbPrefab;

    [Header("Spawn Settings")]
    public int maxActiveOrbs = 3;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;
    public float heightOffset = 0.5f;

    [Header("Orb Lifetime (optional)")]
    public float orbLifetime = 10f;

    private int activeOrbCount = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // wait random time
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(delay);

            // skip if too many orbs active
            if (activeOrbCount >= maxActiveOrbs)
                continue;

            SpawnOrb();
        }
    }

    private void SpawnOrb()
    {
        if (enemyCenter == null || playerMovement == null || orbPrefab == null)
            return;

        float radius = playerMovement.OrbitRadius;

        // random angle around the circle
        float angleDeg = Random.Range(0f, 360f);
        float rad = angleDeg * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * radius;
        Vector3 spawnPos = enemyCenter.position + offset + new Vector3(0f, heightOffset, 0f);

        GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
        activeOrbCount++;

        // give the orb a callback when collected/expired
        EnergyOrb orbScript = orb.GetComponent<EnergyOrb>();
        orbScript.onCollected += HandleOrbRemoved;

        // auto-destroy after lifetime
        if (orbLifetime > 0)
            Destroy(orb, orbLifetime);
    }

    private void HandleOrbRemoved()
    {
        activeOrbCount = Mathf.Max(0, activeOrbCount - 1);
    }
}
