using UnityEngine;

public class SlowOrbit : MonoBehaviour
{
    public float speed = 10f;
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
