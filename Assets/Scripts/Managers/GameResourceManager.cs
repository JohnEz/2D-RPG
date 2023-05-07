using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResourceManager : Singleton<GameResourceManager> {

    [SerializeField]
    private List<Buff> buffs;

    public GameObject circleTelegraphPrefab;

    public Buff GetBuff(string buffName) {
        return buffs.Find(buff => buff.Name == buffName);
    }
}