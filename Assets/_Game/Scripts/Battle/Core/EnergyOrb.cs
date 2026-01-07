using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnergyOrb : MonoBehaviour
{
    [Header("Energy")]
    public int energyAmount = 10;

    [Header("Idle Animation")]
    public float rotateSpeed   = 60f;
    public float bobAmplitude  = 0.25f;
    public float bobFrequency  = 2f;

    public System.Action onCollected;

    private Vector3 basePos;

    private void Awake()
    {
        basePos = transform.position;

        var col = GetComponent<Collider>();
        col.isTrigger = true;   // important for OnTriggerEnter
    }

    private void Update()
    {
        // rotate
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

        // bob up & down
        float y = basePos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(basePos.x, y, basePos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        var energy = other.GetComponent<PlayerEnergy>() 
                    ?? other.GetComponentInParent<PlayerEnergy>();

        if (energy != null)
        {
            energy.GainEnergy(energyAmount);

            // notify spawner
            onCollected?.Invoke();

            // destroy orb
            Destroy(gameObject);
        }
    }
}