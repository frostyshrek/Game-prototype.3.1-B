using UnityEngine;

public class PlayerParryVisual : MonoBehaviour
{
    [Header("Refs")]
    public BattleOrbitMovement movement;   // drag your BattleOrbitMovement here
    public GameObject parryObject;         // drag the ParryVisual child here

    void Update()
    {
        if (movement == null || parryObject == null) return;

        bool shouldBeActive = movement.IsParrying;  // just wraps IsDucking
        if (parryObject.activeSelf != shouldBeActive)
        {
            parryObject.SetActive(shouldBeActive);
        }
    }
}
