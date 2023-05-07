using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal struct CombatTextParams {
    public string text;
    public Color colour;
    public float fontSize;
}

public class UnitCanvasController : MonoBehaviour {
    private const float COMBAT_TEXT_THROTTLE = 0.1f;

    public GameObject combatTextPrefab;
    public GameObject buffIconPrefab;

    public HpBarController hpBar;
    [SerializeField] private CastBarController castBar;
    public Transform UnitFrameTransform;

    private Queue<CombatTextParams> combatTextQueue = new Queue<CombatTextParams>();

    private bool canCreateCombatText = true;

    private UnitController myUnitController;
    private Character myCharacter;
    private int myTeam;

    private Color[] teamColours = new Color[] {
        new Color (0.7294f, 0.9569f, 0.1176f), //green
        new Color(0.8039f, 0.4039f, 0.2039f), //red
        new Color (0, 0.9647f, 1), //blue
        //new Color (0.8431f, 0.2f, 0.2f), //red
    };

    private void Awake() {
        myCharacter = GetComponentInParent<Character>();
        myUnitController = GetComponentInParent<UnitController>();
    }

    private void Start() {
        Initialise();
    }

    private void Update() {
        //TODO there might be a better way to not check for this each time as well
        if (combatTextQueue.Count > 0 && canCreateCombatText) {
            CombatTextParams combatText = combatTextQueue.Dequeue();
            CreateCombatText(combatText.text, combatText.colour, combatText.fontSize);
        }
    }

    public void Initialise() {
        myTeam = (int)myCharacter.faction;

        hpBar.Initialize(myCharacter.GetComponent<NetworkStats>());
        //hpBar.SetHPColor(teamColours[myTeam]);

        castBar.Initialize(myUnitController);

        myCharacter.OnTakeDamage.AddListener(OnTakeDamage);
        myCharacter.OnReceiveHealing.AddListener(CreateHealText);
    }

    private void OnTakeDamage(int damage, bool isShield) {
        if (isShield) {
            CreateShieldText(damage);
        } else {
            CreateDamageText(damage);
        }
    }

    public void CreateDamageText(int damage) {
        float fontSize = CalculateCombatTextFontSize(damage);
        //CreateCombatText(damage.ToString(), new Color(0.8431f, 0.2f, 0.2f), fontSize);
        CreateCombatText(damage.ToString(), new Color(0.845f, 0.8607f, 0.8961f), fontSize);
    }

    public void CreateHealText(int healing) {
        float fontSize = CalculateCombatTextFontSize(healing);
        CreateCombatText(healing.ToString(), new Color(0.7294f, 0.9569f, 0.1176f), fontSize);
    }

    public void CreateShieldText(int shield) {
        CreateCombatText($"({shield.ToString()})", new Color(0.445f, 0.4607f, 0.4961f), CombatText.MIN_FONT_SIZE);
    }

    public void CreateBasicText(string text) {
        CreateCombatText(text, new Color(1f, 1f, 1f), 0.5f);
    }

    private float CalculateCombatTextFontSize(float value) {
        float minValue = 5;
        float maxValue = 30;

        float clampedValue = Mathf.Clamp(value, minValue, maxValue);

        float modValue = clampedValue - minValue;

        float fontSizeDif = CombatText.MAX_FONT_SIZE - CombatText.MIN_FONT_SIZE;

        float percentage = modValue / (maxValue - minValue);

        float fontSize = CombatText.MIN_FONT_SIZE + (percentage * fontSizeDif);

        return fontSize;
    }

    public void CreateCombatText(string text, Color colour, float fontSize) {
        if (canCreateCombatText) {
            SpawnCombatText(text, colour, fontSize);
        } else {
            CombatTextParams newText;
            newText.text = text;
            newText.colour = colour;
            newText.fontSize = fontSize;
            combatTextQueue.Enqueue(newText);
        }
    }

    public void SpawnCombatText(string text, Color color, float fontSize) {
        canCreateCombatText = false;
        GameObject newDamageText = Instantiate(combatTextPrefab, transform);

        //randomise X
        newDamageText.transform.localPosition = new Vector3(Random.value - 0.5f, newDamageText.transform.localPosition.y, newDamageText.transform.localPosition.z);

        newDamageText.GetComponent<CombatText>().Setup(text, color, fontSize);
        StartCoroutine(AllowCreateCombatText());
    }

    private IEnumerator AllowCreateCombatText() {
        yield return new WaitForSeconds(COMBAT_TEXT_THROTTLE);
        canCreateCombatText = true;
    }
}