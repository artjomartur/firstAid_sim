using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Blue Screen of Death (BSOD) effect in classic Windows 98 style.
/// Can be triggered from GameOver, Terminal command, or randomly.
/// </summary>
public class BSODManager : MonoBehaviour
{
    public static BSODManager Instance;

    private GameObject overlay;
    private bool isActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        overlay = new GameObject("BSODOverlay");
        overlay.transform.SetParent(canvasRT, false);

        RectTransform overlayRT = overlay.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        overlayRT.anchoredPosition = Vector2.zero;

        Image bg = overlay.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0.67f, 1f); // Classic BSOD blue
        bg.raycastTarget = true;

        // Title bar
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(overlay.transform, false);
        RectTransform titleBarRT = titleBar.AddComponent<RectTransform>();
        titleBarRT.anchoredPosition = new Vector2(0, 200);
        titleBarRT.sizeDelta = new Vector2(500, 40);

        Image titleBg = titleBar.AddComponent<Image>();
        titleBg.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        titleBg.raycastTarget = false;

        Text titleText = CreateBSODText(titleBar.transform, "TitleText", " First Aid OS ", new Vector2(0, 0), 20);
        titleText.color = new Color(0f, 0f, 0.67f);

        // Error message body
        string errorBody =
            "Ein schwerwiegender Fehler ist aufgetreten.\n\n" +
            "ERSTE_HILFE_EXCEPTION (0x00000FA1D)\n\n" +
            "* Adresse 0xBEEFCAFE in Modul RETTUNG.SYS\n" +
            "* Der Patient konnte nicht stabilisiert werden.\n" +
            "* Fehlercode: KEIN_VERBAND_ANGELEGT\n\n" +
            "Wenn Sie diesen Bildschirm zum ersten Mal sehen,\n" +
            "starten Sie den Computer neu. Wenn dieses Problem\n" +
            "erneut auftritt, üben Sie mehr Erste Hilfe.\n\n" +
            "Technische Informationen:\n\n" +
            "*** STOP: 0x0000009C (0x00000FA1, 0xDEADBEEF,\n" +
            "    0xCAFEBABE, 0x00000112)\n\n\n" +
            "Drücke eine beliebige Taste zum Neustarten...";

        Text bodyText = CreateBSODText(overlay.transform, "ErrorBody", errorBody, new Vector2(0, -30), 16);
        bodyText.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 500);

        overlay.SetActive(false);
    }

    private Text CreateBSODText(Transform parent, string name, string content, Vector2 pos, int fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(700, 40);

        Text txt = obj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.text = content;
        txt.raycastTarget = false;
        return txt;
    }

    public void TriggerBSOD()
    {
        if (isActive || overlay == null) return;

        isActive = true;
        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();

        // Play error sound
        if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound, 0.9f);
        }

        StartCoroutine(WaitForDismiss());
    }

    private IEnumerator WaitForDismiss()
    {
        // Wait a brief moment so the initial keypress doesn't dismiss immediately
        yield return new WaitForSecondsRealtime(1.5f);

        // Wait for any key
        while (true)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }

        Dismiss();
    }

    private void Dismiss()
    {
        isActive = false;
        if (overlay != null) overlay.SetActive(false);

        // Play boot chime as "restart"
        if (AudioManager.Instance != null && AudioManager.Instance.bootChimeSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bootChimeSound, 0.5f);
        }
    }

    /// <summary>Check if BSOD is currently showing</summary>
    public bool IsActive() { return isActive; }
}
