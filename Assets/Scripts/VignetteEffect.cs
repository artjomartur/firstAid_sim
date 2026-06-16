using UnityEngine;
using UnityEngine.UI;

public class VignetteEffect : MonoBehaviour
{
    public static VignetteEffect Instance { get; private set; }

    private Image vignetteImage;
    private bool isActive = false;
    private float pulseSpeed = 2f;
    private float maxIntensity = 0.45f;
    private float currentAlpha = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupVignetteUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupVignetteUI()
    {
        // Find or create GameCanvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject vignetteObj = new GameObject("TimePressureVignette");
        vignetteObj.transform.SetParent(canvas.transform, false);
        vignetteObj.transform.SetAsLastSibling(); // Make sure it sits above standard panels

        RectTransform rt = vignetteObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        vignetteImage = vignetteObj.AddComponent<Image>();
        vignetteImage.raycastTarget = false; // Important: do NOT block UI interactions!
        
        // Procedural vignette asset or solid red borders
        // We will proceduralize a vignette look by setting up a transparent red overlay
        // and using a soft fade.
        vignetteImage.color = new Color(0.9f, 0.1f, 0.1f, 0f);

        // Try to load a soft vignette sprite if available, otherwise fallback to transparent color-overlay
        Sprite vignetteSprite = Resources.Load<Sprite>("vignette_shadow");
        if (vignetteSprite != null)
        {
            vignetteImage.sprite = vignetteSprite;
        }

        vignetteObj.SetActive(false);
    }

    public void TriggerVignette(bool active, float speed = 3f, float intensity = 0.5f)
    {
        isActive = active;
        pulseSpeed = speed;
        maxIntensity = intensity;

        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
            if (!active)
            {
                vignetteImage.gameObject.SetActive(false);
                vignetteImage.color = new Color(vignetteImage.color.r, vignetteImage.color.g, vignetteImage.color.b, 0f);
            }
        }
    }

    private void Update()
    {
        if (!isActive || vignetteImage == null) return;

        // Pulse alpha dynamically using a Sine wave
        float targetAlpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f * maxIntensity;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * 5f);

        vignetteImage.color = new Color(vignetteImage.color.r, vignetteImage.color.g, vignetteImage.color.b, currentAlpha);
    }
}
