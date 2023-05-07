using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.Linq;

public enum CharacterStat {
    SHIELD,
    MOVE_SPEED,
    MAX_HEALTH,
    DAMAGE_TAKEN_MOD
}

public class NetworkStat<T> : NetworkVariable<T> where T : unmanaged {

    // needs to be networked?
    public T BaseValue { get; set; }

    public void Reset() {
        Debug.Log(BaseValue);
        Value = BaseValue;
    }
}

public class NetworkStats : NetworkBehaviour {

    [HideInInspector]
    public NetworkVariable<int> MaxHealth = new NetworkVariable<int>();

    //public NetworkStat<int> MaxHealth = new NetworkStat<int>();

    public int BaseMaxHealth = 100;

    [HideInInspector]
    public NetworkVariable<int> Health = new NetworkVariable<int>();

    [HideInInspector]
    public NetworkVariable<int> Shield = new NetworkVariable<int>();

    [HideInInspector]
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>();

    //public NetworkStat<float> MoveSpeed = new NetworkStat<float>();

    public float BaseMoveSpeed = 1f;

    [HideInInspector]
    public NetworkVariable<bool> IsStunned = new NetworkVariable<bool>();

    public float DamageTakenMod { get; set; }

    public event Action HealthDepleted;

    public event Action HealthReplenished;

    public event Action HealthChanged;

    private void Start() {
        DamageTakenMod = 1;
    }

    private void OnEnable() {
        Health.OnValueChanged += HitPointsChanged;
        Health.OnValueChanged += HealthStatChanged;
        Shield.OnValueChanged += HealthStatChanged;
        MaxHealth.OnValueChanged += HealthStatChanged;
    }

    private void OnDisable() {
        Health.OnValueChanged -= HitPointsChanged;
        Health.OnValueChanged -= HealthStatChanged;
        Shield.OnValueChanged -= HealthStatChanged;
        MaxHealth.OnValueChanged -= HealthStatChanged;
    }

    private void HitPointsChanged(int previousValue, int newValue) {
        if (previousValue > 0 && newValue <= 0) {
            // newly reached 0 HP
            HealthDepleted?.Invoke();
            if (NetworkManager.Singleton.IsServer) {
                NetworkObject.Despawn();
            }
        } else if (previousValue <= 0 && newValue > 0) {
            // newly revived
            HealthReplenished?.Invoke();
        }
    }

    private void HealthStatChanged(int previousValue, int newValue) {
        HealthChanged?.Invoke();
    }

    public void ResetStats() {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        //MaxHealth.Reset();
        //MoveSpeed.Reset();
        MaxHealth.Value = BaseMaxHealth;
        MoveSpeed.Value = BaseMoveSpeed;
        Shield.Value = 0;
        IsStunned.Value = false;
        Health.Value = MaxHealth.Value;
        DamageTakenMod = 1;
    }

    public void CalculateStats(List<Buff> buffs) {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        IsStunned.Value = CalculateIsStunned(buffs);

        List<StatBuff> statBuffs = buffs
            .Where(buff => buff is StatBuff)
            .Select(buff => (StatBuff)buff)
            .ToList();

        Shield.Value = GetStatMod(statBuffs, CharacterStat.SHIELD, 0);
        MaxHealth.Value = GetStatMod(statBuffs, CharacterStat.MAX_HEALTH, BaseMaxHealth);
        MoveSpeed.Value = GetStatMod(statBuffs, CharacterStat.MOVE_SPEED, BaseMoveSpeed);
        //MaxHealth.Value = GetStatMod(statBuffs, CharacterStat.MAX_HEALTH, MaxHealth.BaseValue);
        //MoveSpeed.Value = GetStatMod(statBuffs, CharacterStat.MOVE_SPEED, MoveSpeed.BaseValue);
        DamageTakenMod = GetStatMod(statBuffs, CharacterStat.DAMAGE_TAKEN_MOD, 1f);
    }

    private bool CalculateIsStunned(List<Buff> buffs) {
        return buffs.Exists(buff => buff.IsAStun);
    }

    private float GetStatMod(List<StatBuff> buffs, CharacterStat stat, float baseValue) {
        int flatMod = 0;
        float percentageMod = 1;

        buffs.ForEach(statBuff => {
            flatMod += statBuff.GetStatModFlat(stat);
            percentageMod *= statBuff.GetStatModPercent(stat);
        });

        return (baseValue * percentageMod) + flatMod;
    }

    private int GetStatMod(List<StatBuff> buffs, CharacterStat stat, int baseValue) {
        return (int)GetStatMod(buffs, stat, (float)baseValue);
    }
}