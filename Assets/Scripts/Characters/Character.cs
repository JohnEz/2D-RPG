using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public enum CharacterFaction {
    PLAYER,
    ENEMY,
    ALLY,
}

public class Character : NetworkBehaviour {
    public CharacterFaction faction;

    private CharacterStateController characterState;
    protected UnitController unitController;
    protected BuffController buffController;
    protected NetworkCharacterActions characterActions;

    public UnityEvent<int, bool> OnTakeDamage = new UnityEvent<int, bool>();

    public UnityEvent<int> OnReceiveHealing = new UnityEvent<int>();

    private NetworkStats m_networkStats;

    public int Health {
        get { return m_networkStats.Health.Value; }
    }

    public int Shield {
        get { return m_networkStats.Shield.Value; }
    }

    public int MaxHealth {
        get { return m_networkStats.Shield.Value; }
    }

    public float MoveSpeed {
        get { return m_networkStats.MoveSpeed.Value; }
    }

    public bool IsStunned {
        get { return m_networkStats.IsStunned.Value; }
    }

    public List<Ability> abilities;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        abilities = new List<Ability>();

        unitController = GetComponent<UnitController>();
        characterState = GetComponent<CharacterStateController>();
        buffController = GetComponent<BuffController>();
        m_networkStats = GetComponent<NetworkStats>();
        characterActions = GetComponent<NetworkCharacterActions>();

        buffController.OnBuffsChanged.AddListener(CalculateStats);

        m_networkStats.IsStunned.OnValueChanged += OnStunnedChanged;
    }

    private void Start() {
        unitController = GetComponent<UnitController>();
        characterState = GetComponent<CharacterStateController>();
        buffController = GetComponent<BuffController>();
        m_networkStats = GetComponent<NetworkStats>();

        buffController.OnBuffsChanged.AddListener(CalculateStats);

        m_networkStats.IsStunned.OnValueChanged += OnStunnedChanged;

        m_networkStats.ResetStats();
    }

    public void SetBaseStats(int baseMaxHealth, float baseMoveSpeed) {
        //m_networkStats.MaxHealth.BaseValue = baseMaxHealth;
        //m_networkStats.MoveSpeed.BaseValue = baseMoveSpeed;
        m_networkStats.BaseMaxHealth = baseMaxHealth;
        m_networkStats.BaseMoveSpeed = baseMoveSpeed;
        m_networkStats.MaxHealth.Value = baseMaxHealth;
        m_networkStats.MoveSpeed.Value = baseMoveSpeed;
    }

    public void CalculateStats(List<Buff> currentBuffs) {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        m_networkStats.CalculateStats(currentBuffs);
    }

    private void OnStunnedChanged(bool previousIsStunned, bool newIsStunned) {
        if (newIsStunned) {
            if (NetworkManager.Singleton.IsServer) {
                unitController.CancelCastServerRpc(false);
            }
            characterState.State = CharacterState.Stunned;
        } else if (characterState.IsStunned()) {
            characterState.State = CharacterState.Idle;
        }
    }

    private void Update() {
    }

    public int TakeDamage(int damage) {
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Client tried to call TakeDamage");
            return 0;
        }

        int modifiedDamage = (int)(damage * m_networkStats.DamageTakenMod);
        int remainingDamage = modifiedDamage;

        if (Shield > 0) {
            int initialDamage = remainingDamage;

            List<ShieldBuff> shields = buffController.ActiveBuffs
                .FindAll(buff => buff is ShieldBuff)
                .Select(buff => (ShieldBuff)buff)
                .OrderBy(buff => buff.RemainingTime())
                .ToList();

            int index = 0;
            while (index < shields.Count && remainingDamage > 0) {
                remainingDamage = shields[index].TakeDamage(remainingDamage);
                index++;
            }

            int damageAbsorbed = initialDamage - remainingDamage;

            if (damageAbsorbed > 0) {
                TakeDamageClientRpc(damageAbsorbed, true);

                CalculateStats(buffController.ActiveBuffs);
            }
        }

        if (remainingDamage > 0) {
            int newHealth = Mathf.Max(0, m_networkStats.Health.Value - remainingDamage);
            m_networkStats.Health.Value = newHealth;

            TakeDamageClientRpc(remainingDamage, false);
        }

        return modifiedDamage;
    }

    [ClientRpc]
    public void TakeDamageClientRpc(int damage, bool hitShield) {
        OnTakeDamage.Invoke(damage, hitShield);
    }

    public void ReceiveHealing(int healing) {
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Client tried to call ReceiveHealing");
            return;
        }

        int newHealth = Mathf.Min(m_networkStats.MaxHealth.Value, m_networkStats.Health.Value + healing);
        m_networkStats.Health.Value = newHealth;

        ReceiveHealingClientRpc(healing);
    }

    [ClientRpc]
    public void ReceiveHealingClientRpc(int healing) {
        OnReceiveHealing.Invoke(healing);
    }

    // move to statics to a utils

    public static List<Character> GetCircleHitTargets(Vector3 worldPos, float radius, CharacterFaction casterFaction, bool hitsFriends = false) {
        List<Collider2D> hitTargets = new List<Collider2D>(Physics2D.OverlapCircleAll(new Vector2(worldPos.x, worldPos.y), radius));

        return hitTargets.Where(collider => {
            Character characterInRange = collider.gameObject.GetComponent<Character>();

            if (!characterInRange) {
                return false;
            }

            bool isAlly = casterFaction == characterInRange.faction;

            return hitsFriends == isAlly;
        }).Select(collider => collider.gameObject.GetComponent<Character>()).ToList();
    }

    public static Character GetClosestUnitInRadius(Vector3 worldPos, CharacterFaction casterFaction, bool getAlly = true, float radius = 3f) {
        List<Character> characters = GetCircleHitTargets(worldPos, radius, casterFaction, getAlly);

        if (characters.Count < 1) {
            return null;
        }

        Character closestCharacter = characters
            .OrderBy(character => Vector3.Distance(worldPos, character.transform.position))
            .First();

        return closestCharacter;
    }
}