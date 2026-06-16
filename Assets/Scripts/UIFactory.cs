using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Senior Developer Tool: A factory class to handle UI creation and styling.
/// Decouples UI construction from game logic.
/// </summary>
public static class UIFactory
{
    public static GameObject CreateFullscreenPanel(RectTransform parent, string name, Color col)
    {
        GameObject obj = CreateUIElement(parent, name, Vector2.zero, Vector2.zero);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        obj.GetComponent<Image>().color = col;
        return obj;
    }

    public static GameObject CreateUIElement(RectTransform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        obj.AddComponent<Image>();
        return obj;
    }

    public static Text CreateText(Transform parent, string name, string content, Vector2 pos, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1000, 200);
        
        Text txt = obj.AddComponent<Text>();
        txt.raycastTarget = false; 
        txt.text = content;
        txt.fontSize = fontSize;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = alignment;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);
        
        return txt;
    }

    public static Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor, Sprite bgSprite = null, Sprite fillSprite = null)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        
        Image bgImg = obj.AddComponent<Image>();
        bgImg.color = bgSprite != null ? Color.white : new Color(0.1f, 0.1f, 0.1f, 0.8f);
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.type = Image.Type.Sliced;
        }
        
        Slider slider = obj.AddComponent<Slider>();
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(5, 5);
        fillRect.offsetMax = new Vector2(-5, -5);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = fillSprite != null ? Color.white : fillColor;
        if (fillSprite != null)
        {
            fillImg.sprite = fillSprite;
            fillImg.type = Image.Type.Sliced;
        }
        
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.fillRect.anchorMin = Vector2.zero;
        slider.fillRect.anchorMax = Vector2.one;
        slider.fillRect.sizeDelta = Vector2.zero;
        
        return slider;
    }

    public static void SetupImage(GameObject obj, Sprite s, bool preserveAspect)
    {
        Image img = obj.GetComponent<Image>();
        if (img == null) img = obj.AddComponent<Image>();
        
        if (s != null)
        {
            img.sprite = s;
            img.preserveAspect = preserveAspect;
            img.color = Color.white;
            if (!preserveAspect) img.type = Image.Type.Sliced;
        }
    }

    public static void AddHoverEffect(GameObject btn, AudioClip hoverSound = null, AudioSource source = null)
    {
        ButtonHoverScaler scaler = btn.GetComponent<ButtonHoverScaler>();
        if (scaler == null) scaler = btn.AddComponent<ButtonHoverScaler>();
        scaler.hoverSound = hoverSound;
        scaler.sfxSource = source;
    }

    public static GameObject CreateButton(RectTransform parent, string name, string textContent, Vector2 pos, Vector2 size, Sprite bgSprite = null)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        if (bgSprite != null)
        {
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        
        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        RectTransform txtRt = textObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        Text txt = textObj.AddComponent<Text>();
        txt.raycastTarget = false; // Prevents blocking of raycasts to the parent button!
        txt.text = textContent;
        txt.fontSize = 24;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;

        AddHoverEffect(obj);

        return obj;
    }
}

public class ButtonHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip hoverSound;
    public AudioSource sfxSource;
    private Vector3 originalScale = Vector3.one;

    private void Start()
    {
        originalScale = transform.localScale;
        if (sfxSource == null && AudioManager.Instance != null)
        {
            sfxSource = AudioManager.Instance.sfxSource;
        }
        if (hoverSound == null)
        {
            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null) hoverSound = bootstrap.buttonHoverSound;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.05f;
        if (hoverSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
    }
}
