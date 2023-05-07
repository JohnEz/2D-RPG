using UnityEngine;
using System.Collections;
using UnityEngine.VFX;
using Unity.Netcode;

public class Telegraph : MonoBehaviour {
    private VisualEffect myVFX;
    private Ability.OnHitDelegate OnHit;

    [SerializeField] private AudioClip onCastSFX;
    [SerializeField] private AudioClip onImpactSFX;

    private void Awake() {
        myVFX = GetComponent<VisualEffect>();
    }

    public void Initialise(float impactDelay, float radius, Ability.OnHitDelegate abilityOnHit) {
        myVFX.SetFloat("Size", radius * 2);
        myVFX.SetFloat("Impact_Time", impactDelay);
        OnHit = abilityOnHit;

        AudioManager.Instance.PlaySound(onCastSFX, transform.position);

        Invoke("OnImpact", impactDelay);
    }

    private void OnImpact() {
        AudioManager.Instance.PlaySound(onImpactSFX, transform.position);

        if (NetworkManager.Singleton.IsServer && OnHit != null) {
            OnHit(null, transform.position);
        }

        Destroy(gameObject, 2f);
    }

    public static GameObject CreateCircleTelegraph(Vector3 position, float impactDelay, float radius, Ability.OnHitDelegate abilityOnHit) {
        GameObject telegraph = Instantiate(GameResourceManager.Instance.circleTelegraphPrefab);
        telegraph.transform.position = position;
        telegraph.GetComponent<Telegraph>().Initialise(impactDelay, radius, abilityOnHit);

        return telegraph;
    }
}