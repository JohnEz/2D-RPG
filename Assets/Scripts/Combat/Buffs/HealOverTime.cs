using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New HealOverTime", menuName = "RPG/Buff/HealOverTime")]
public class HealOverTime : Buff {

    [SerializeField]
    private int power;

    public override void OnTick() {
        base.OnTick();

        targetCharacter.ReceiveHealing(power);
    }
}