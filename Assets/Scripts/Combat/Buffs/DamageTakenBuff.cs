using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New DamageTakenBuff", menuName = "RPG/Buff/DamageTakenBuff")]
public class DamageTakenBuff : StatBuff {

    [SerializeField]
    private float damageTakenMod;

    public override void Initailise(Character target) {
        base.Initailise(target);
        statModsPercent[CharacterStat.DAMAGE_TAKEN_MOD] = 1 + damageTakenMod;
    }
}