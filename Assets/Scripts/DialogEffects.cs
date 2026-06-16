using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Smoothly pulsates the color of an accent line between warm colors
/// </summary>
public class AccentPulse : MonoBehaviour
{
    private Image img;
    private float hue = 0f;

    void Start()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        if (img == null) return;
        hue += Time.unscaledDeltaTime * 0.08f;
        if (hue > 1f) hue -= 1f;
        img.color = Color.HSVToRGB(hue, 0.7f, 1f);
    }
}

/// <summary>
/// Pop-in scale animation for dialog windows (elastic ease-out)
/// </summary>
public class DialogPopIn : MonoBehaviour
{
    private RectTransform rt;
    private float elapsed = 0f;
    private float duration = 0.35f;
    private bool done = false;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.zero;
        elapsed = 0f;
        done = false;

        if (AudioManager.Instance != null && AudioManager.Instance.windowOpenSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.windowOpenSound, 0.4f);
        }
    }

    void Update()
    {
        if (done || rt == null) return;

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        
        // Elastic ease-out for a premium bouncy feel
        float scale;
        if (t < 0.6f)
        {
            // Fast growth phase
            scale = Mathf.Sin(t / 0.6f * Mathf.PI * 0.5f) * 1.06f;
        }
        else
        {
            // Settle phase
            float settle = (t - 0.6f) / 0.4f;
            scale = 1.06f - (0.06f * settle);
        }
        
        rt.localScale = new Vector3(scale, scale, 1f);

        if (t >= 1f)
        {
            rt.localScale = Vector3.one;
            done = true;
        }
    }
}

/// <summary>
/// Pop-out scale animation for dialog windows (shrink and disappear)
/// </summary>
public class DialogPopOut : MonoBehaviour
{
    private RectTransform rt;
    private float elapsed = 0f;
    private float duration = 0.2f;
    private System.Action onComplete;

    public static void Trigger(GameObject target, System.Action onComplete = null)
    {
        if (target == null) return;
        DialogPopOut pop = target.GetComponent<DialogPopOut>();
        if (pop == null) pop = target.AddComponent<DialogPopOut>();
        
        pop.onComplete = onComplete;
        pop.elapsed = 0f;

        if (AudioManager.Instance != null && AudioManager.Instance.windowCloseSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.windowCloseSound, 0.4f);
        }
    }

    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rt == null) return;

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Ease in back
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        float scale = 1f - (c3 * t * t * t - c1 * t * t);
        
        if (scale < 0f) scale = 0f;

        rt.localScale = new Vector3(scale, scale, 1f);

        if (t >= 1f)
        {
            if (onComplete != null) onComplete();
            else gameObject.SetActive(false);
            Destroy(this);
        }
    }
}
