using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New ShieldBuff", menuName = "RPG/Buff/ShieldBuff")]
public class ShieldBuff : StatBuff {
    [SerializeField] private int shieldAmount;

    public override void Initailise(Character target) {
        base.Initailise(target);
        statModsFlat[CharacterStat.SHIELD] = shieldAmount;
    }

    public override bool HasExpired() {
        return statModsFlat[CharacterStat.SHIELD] <= 0 || base.HasExpired();
    }

    public int TakeDamage(int damage) {
        int damageToTake = Mathf.Min(damage, statModsFlat[CharacterStat.SHIELD]);

        statModsFlat[CharacterStat.SHIELD] -= damageToTake;

        int remainingDamage = damage - damageToTake;

        return remainingDamage;
    }
}