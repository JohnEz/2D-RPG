using UnityEngine;
using UnityEditor;
using Unity.Netcode;

[System.Serializable]
public class Buff : ScriptableObject {

    [SerializeField]
    private string _name;

    public string Name {
        get { return _name; }
        set { _name = value; }
    }

    [SerializeField]
    private float _duration;

    public float Duration {
        get { return _duration; }
        set { _duration = value; }
    }

    [SerializeField]
    private float _interval;

    public float Interval {
        get { return _interval; }
        set { _interval = value; }
    }

    public float ElapsedTime { get; set; }

    protected Character targetCharacter;

    private int tickCounter = 0;

    [SerializeField]
    private bool _isAStun;

    public bool IsAStun {
        get { return _isAStun; }
        set { _isAStun = value; }
    }

    [SerializeField]
    private AudioClip applySFX;

    [SerializeField]
    private AudioClip expireSFX;

    [SerializeField]
    private GameObject applyVFX;

    [SerializeField]
    private GameObject expireVFX;

    public virtual void Initailise(Character target) {
        targetCharacter = target;
        ElapsedTime = 0;

        if (applySFX) {
            AudioManager.Instance.PlaySound(applySFX, targetCharacter.transform.position);
        }

        if (applyVFX) {
            Instantiate(applyVFX, target.transform);
        }
    }

    public virtual void UpdateElapsedTime(float deltaTime) {
        ElapsedTime += deltaTime;

        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        int ticks = Mathf.FloorToInt(ElapsedTime / Interval);

        if (ticks > tickCounter) {
            OnTick();
            tickCounter++;
        }
    }

    public virtual bool HasExpired() {
        return ElapsedTime >= Duration;
    }

    public virtual void OnTick() {
    }

    public virtual void OnExpire() {
        if (expireSFX) {
            AudioManager.Instance.PlaySound(expireSFX, targetCharacter.transform.position);
        }

        if (expireVFX) {
            Instantiate(expireVFX, targetCharacter.transform);
        }
    }

    public virtual bool ShouldBeOverriden(Buff newBuff) {
        return newBuff.Duration > RemainingTime();
    }

    public float RemainingTime() {
        return Duration - ElapsedTime;
    }
}