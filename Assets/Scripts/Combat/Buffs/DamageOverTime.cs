using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New DamageOverTime", menuName = "RPG/Buff/DamageOverTime")]
public class DamageOverTime : Buff {

    [SerializeField]
    private int power;

    public override void OnTick() {
        base.OnTick();

        targetCharacter.TakeDamage(power);
    }
}