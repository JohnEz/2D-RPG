using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New SlowDebuff", menuName = "RPG/Buff/SlowDebuff")]
public class SlowDebuff : StatBuff {

    [SerializeField]
    private float slowPercent;

    public override void Initailise(Character target) {
        base.Initailise(target);
        statModsPercent[CharacterStat.MOVE_SPEED] = 1 - slowPercent;
    }
}