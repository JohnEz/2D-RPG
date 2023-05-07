using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class BuffController : NetworkBehaviour {
    private Character myCharacter;
    public UnityEvent<List<Buff>> OnBuffsChanged = new UnityEvent<List<Buff>>();

    private List<Buff> _activeBuffs;

    public List<Buff> ActiveBuffs {
        get { return _activeBuffs; }
        set { _activeBuffs = value; OnBuffsChanged.Invoke(_activeBuffs); }
    }

    private void Awake() {
        myCharacter = GetComponent<Character>();
        ActiveBuffs = new List<Buff>();
    }

    private void Update() {
        List<Buff> expiredBuffs = new List<Buff>();
        ActiveBuffs.ForEach(buff => {
            buff.UpdateElapsedTime(Time.deltaTime);

            if (buff.HasExpired()) {
                expiredBuffs.Add(buff);
            }
        });

        RemoveExpiredBuffs(expiredBuffs);
    }

    private void RemoveExpiredBuffs(List<Buff> expiredBuffs) {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        if (expiredBuffs.Count > 0) {
            expiredBuffs.ForEach(expiredBuff => {
                expiredBuff.OnExpire();
                RemoveBuffClientRpc(expiredBuff.Name);
            });
        }
    }

    public bool HasBuff(string buffName) {
        return ActiveBuffs.Exists(buff => buff.Name.Equals(buffName));
    }

    public bool HasBuff(Buff buffToCheck) {
        return HasBuff(buffToCheck.Name);
    }

    public Buff GetBuff(string buffName) {
        return ActiveBuffs.Find(buff => buff.Name.Equals(buffName));
    }

    public Buff GetBuff(Buff buffPrefab) {
        return GetBuff(buffPrefab.Name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyBuffServerRpc(string buffName) {
        ApplyBuff(buffName);
        ApplyBuffClientRpc(buffName);
    }

    [ClientRpc]
    private void ApplyBuffClientRpc(string buffName) {
        ApplyBuff(buffName);
    }

    private void ApplyBuff(string buffName) {
        Buff appliedBuff = GameResourceManager.Instance.GetBuff(buffName);

        ApplyBuff(appliedBuff);
    }

    private void ApplyBuff(Buff buff) {
        Buff newBuff = Instantiate(buff);
        newBuff.Initailise(myCharacter);

        Buff originalBuff = GetBuff(newBuff.Name);
        if (originalBuff) {
            if (!originalBuff.ShouldBeOverriden(newBuff)) {
                return;
            }

            ActiveBuffs.RemoveAll(existingBuff => existingBuff.Name == buff.Name);
        }

        ActiveBuffs.Add(newBuff);

        OnBuffsChanged.Invoke(ActiveBuffs);
    }

    [ServerRpc]
    public void RemoveBuffServerRpc(string buffName) {
        RemoveBuff(buffName);
        RemoveBuffClientRpc(buffName);
    }

    [ClientRpc]
    private void RemoveBuffClientRpc(string buffName) {
        RemoveBuff(buffName);
    }

    private void RemoveBuff(string buffName) {
        ActiveBuffs = ActiveBuffs.FindAll(buff => buff.Name != buffName).ToList();

        OnBuffsChanged.Invoke(ActiveBuffs);
    }

    public void UpdateBuffDuration(string buffName, float mod) {
        if (!NetworkManager.Singleton.IsServer || !HasBuff(buffName)) {
            return;
        }

        Buff currentBuff = GetBuff(buffName);
        currentBuff.Duration += mod;

        IncreaseBuffDurationClientRpc(buffName, currentBuff.Duration);
    }

    [ClientRpc]
    private void IncreaseBuffDurationClientRpc(string buffName, float newDuration) {
        if (!HasBuff(buffName)) {
            // TODO maybe add it as it must exist on server?
            // does that risk adding it after server removes it?
            return;
        }

        GetBuff(buffName).Duration = newDuration;
    }
}