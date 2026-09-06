using UnityEngine;
using UnityEngine.SceneManagement;

// Added to the main camera by PlayerHealth.Die().
// Requires no Canvas, no EventSystem, no scene setup.
public class AnimatedDeathScreen : MonoBehaviour
{
    private Sprite[] frames;
    private int frameIndex;
    private float frameTimer;
    private const float FPS = 5f;
    private int wavesReached;

    void Awake()
    {
        var data = Resources.Load<DeathScreenData>("DeathScreenData");
        frames      = data != null ? data.frames : null;
        wavesReached = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        frameTimer += Time.unscaledDeltaTime;
        if (frameTimer >= 1f / FPS)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % frames.Length;
        }
    }

    void OnGUI()
    {
        float sw = Screen.width, sh = Screen.height;

        // Dark overlay
        var prevColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.88f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = prevColor;

        // Animated "You're dead" sprite
        if (frames != null && frames.Length > 0)
        {
            var sprite = frames[frameIndex];
            float aspect = sprite.rect.width / sprite.rect.height;
            float imgH   = sh * 0.38f;
            float imgW   = imgH * aspect;
            var imgRect  = new Rect(sw / 2f - imgW / 2f, sh / 2f - imgH / 2f - 80f, imgW, imgH);

            // Draw sub-sprite from the sheet using normalized UV coords
            var uvRect = new Rect(
                sprite.textureRect.x      / sprite.texture.width,
                sprite.textureRect.y      / sprite.texture.height,
                sprite.textureRect.width  / sprite.texture.width,
                sprite.textureRect.height / sprite.texture.height
            );
            GUI.DrawTextureWithTexCoords(imgRect, sprite.texture, uvRect);
        }

        // Wave label
        if (wavesReached > 0)
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 22,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white }
            };
            GUI.Label(new Rect(sw / 2f - 220f, sh / 2f + 30f, 440f, 40f),
                $"You reached wave {wavesReached}!", labelStyle);
        }

        // Buttons
        const float btnW = 170f, btnH = 55f, gap = 24f;
        float bx1 = sw / 2f - btnW - gap / 2f;
        float bx2 = sw / 2f + gap / 2f;
        float by  = sh * 0.82f;

        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };

        if (GUI.Button(new Rect(bx1, by, btnW, btnH), "Restart", btnStyle))
        {
            visible = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (GUI.Button(new Rect(bx2, by, btnW, btnH), "Main Menu", btnStyle))
        {
            visible = false;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
