using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarController : MonoBehaviour {
    public Image hpBar;
    public Image shieldBar;
    public Image damageBar;

    public bool displayHPMarkers = true;

    [SerializeField]
    public GameObject hpMarkerPrefab;

    private List<GameObject> hpMarkers = new List<GameObject>();

    private const int HP_MARKER_INTERVAL = 10;
    private const float DAMAGE_BAR_SHRINK_TIMER_MAX = 0.5f;

    private float damageBarShrinkTimer;
    private float currentMax = 0;
    private float targetHPPercent = 1;
    private float targetShieldPercent = 0;

    private NetworkStats myStats;

    public void Initialize(NetworkStats stats) {
        myStats = stats;
        currentMax = stats.MaxHealth.Value;
        if (displayHPMarkers) {
            CreateMarkers(currentMax);
        }

        myStats.HealthChanged += SetHp;
    }

    public void OnDisable() {
        if (myStats != null) {
            myStats.HealthChanged -= SetHp;
        }
    }

    // Update is called once per frame
    private void Update() {
        //UpdateBarToValue(shieldBar, targetShieldPercent);
        //UpdateBarToValue(hpBar, targetHPPercent);

        if (damageBarShrinkTimer < 0) {
            UpdateBarToValue(damageBar, hpBar.fillAmount);
        } else {
            damageBarShrinkTimer -= Time.deltaTime;
        }
    }

    public static void UpdateBarToValue(Image bar, float targetAmount) {
        if (targetAmount == bar.fillAmount) {
            return;
        }
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, targetAmount, 5f * Time.deltaTime);

        float distance = Mathf.Abs(targetAmount - bar.fillAmount);
        if (distance < 0.005f) {
            bar.fillAmount = targetAmount;
        }
    }

    public void SetHp() {
        float health = myStats.Health.Value;
        float shield = myStats.Shield.Value;
        float maxHealth = myStats.MaxHealth.Value;

        targetHPPercent = health / (maxHealth + shield);
        targetShieldPercent = (health + shield) / (maxHealth + shield);

        hpBar.fillAmount = targetHPPercent;
        shieldBar.fillAmount = targetShieldPercent;

        damageBarShrinkTimer = DAMAGE_BAR_SHRINK_TIMER_MAX;

        if (displayHPMarkers) {
            if (maxHealth != currentMax + shield) {
                currentMax = maxHealth + shield;
                DestroyMarkers();
                CreateMarkers(maxHealth + shield);
            }
        }
    }

    public void SetHPColor(Color color) {
        hpBar.color = color;
    }

    public void CreateMarkers(float maxHp) {
        int numberOfMarkers = (int)(maxHp / HP_MARKER_INTERVAL);
        float increment = hpBar.rectTransform.rect.width / (numberOfMarkers);

        for (int i = 0; i < numberOfMarkers - 1; i++) {
            GameObject newMarker = CreateMarker(i, increment);
            hpMarkers.Add(newMarker);
        }
    }

    public GameObject CreateMarker(int index, float increment) {
        GameObject newMarker = Instantiate(hpMarkerPrefab);
        Vector3 newPosition = newMarker.transform.position;
        newMarker.transform.SetParent(hpBar.transform, false);
        newPosition.x = Mathf.RoundToInt((index + 1) * increment) - (hpBar.rectTransform.rect.width / 2);
        newMarker.GetComponent<RectTransform>().anchoredPosition = newPosition;
        return newMarker;
    }

    private void UpdateShieldBarPositionLEGACY() {
        float hpBarEnd = hpBar.rectTransform.rect.width * hpBar.fillAmount;
        shieldBar.rectTransform.anchoredPosition = new Vector3(hpBarEnd - 1, shieldBar.rectTransform.anchoredPosition.y, 0);
    }

    public void DestroyMarkers() {
        hpMarkers.ForEach((hpMarker) => {
            Destroy(hpMarker);
        });
        hpMarkers.Clear();
    }
}