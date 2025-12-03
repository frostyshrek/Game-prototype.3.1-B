using UnityEngine;
using BattleSystem;   // for CardAttribute if you want to use it later

[CreateAssetMenu(fileName = "New Enemy Attack Pattern", menuName = "Battle System/Enemy Attack Pattern")]
public class EnemyAttackPattern : ScriptableObject
{
    [Header("Basic Info")]
    public string attackName = "Slash";
    public int damage = 10;

    [Header("Timing")]
    public float telegraphTime = 1.2f;    // warning time
    public float recoverTime = 0.5f;      // pause after attack before next

    [Header("Dodge Requirement")]
    public RequiredDodge requiredDodge = RequiredDodge.Jump;

    [Header("Visuals / Feedback")]
    public Sprite icon;                   // for telegraph UI
    public GameObject telegraphVFXPrefab; // optional
    public GameObject hitVFXPrefab;       // optional

    [Header("Attribute")]
    public CardAttribute attribute = CardAttribute.None;
}
