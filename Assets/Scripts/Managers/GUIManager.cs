using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : Singleton<GUIManager> {

    [SerializeField]
    private Texture2D crosshairTexture;

    [SerializeField]
    private GameObject cooldownTextPrefab;

    [SerializeField]
    private Canvas canvas;

    private const float COOLDOWN_TEXT_THROTTLE = 1.7f;
    private float cooldownTextSpawnTime = 0;

    // Start is called before the first frame update
    private void Start() {
        Vector2 cursorOffset = new Vector2(crosshairTexture.width / 2, crosshairTexture.height / 2);

        Cursor.SetCursor(crosshairTexture, cursorOffset, CursorMode.Auto);
    }

    public void CreateCooldownText(Sprite icon, string text) {
        if (cooldownTextSpawnTime + COOLDOWN_TEXT_THROTTLE > Time.time) {
            return;
        }

        cooldownTextSpawnTime = Time.time;

        var cooldownText = Instantiate(cooldownTextPrefab, Input.mousePosition, Quaternion.identity, canvas.transform);

        cooldownText.GetComponent<CooldownText>().Setup(icon, text);
    }
}