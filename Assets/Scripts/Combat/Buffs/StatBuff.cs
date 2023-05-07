using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public abstract class StatBuff : Buff {
    protected Dictionary<CharacterStat, int> statModsFlat = new Dictionary<CharacterStat, int>();
    protected Dictionary<CharacterStat, float> statModsPercent = new Dictionary<CharacterStat, float>();

    public int GetStatModFlat(CharacterStat stat) {
        if (statModsFlat.ContainsKey(stat)) {
            return statModsFlat[stat];
        }

        return 0;
    }

    public float GetStatModPercent(CharacterStat stat) {
        if (statModsPercent.ContainsKey(stat)) {
            return statModsPercent[stat];
        }

        return 1;
    }
}