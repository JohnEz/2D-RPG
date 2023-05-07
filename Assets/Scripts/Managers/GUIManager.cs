using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : Singleton<GUIManager> {
    public Texture2D crosshairTexture;

    // Start is called before the first frame update
    private void Start() {
        Vector2 cursorOffset = new Vector2(crosshairTexture.width / 2, crosshairTexture.height / 2);

        Cursor.SetCursor(crosshairTexture, cursorOffset, CursorMode.Auto);
    }
}