using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks player tiredness and handles sleeping at the bed.
/// Tiredness rises passively. At 100% the player moves slower.
/// Press E near the bed to sleep → fade → reset tiredness, advance time by 1/3 day.
/// </summary>
public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance { get; private set; }

    [Header("Tiredness Settings")]
    public float maxTiredness    = 100f;
    public float tirednessPerMin = 3f;     // passive gain per real minute
    public float speedPenaltyPct = 0.5f;  // speed multiplier when exhausted

    [Header("Bed Interaction")]
    public Transform bedTransform;
    public float bedInteractRadius = 1.5f;
    public Sprite bedSprite;

    [Header("HUD")]
    public RectTransform tirednessBarFill;   // filled from left, set by GameBootstrap
    public Image tirednessIcon;              // moon icon, optional

    [System.NonSerialized] public float currentTiredness = 0f;
    private bool isSleeping = false;
    private PlayerController playerCtrl;
    private float originalSpeed;
    private bool speedReduced = false;
    private GameObject sleepHintUI;
    private GameObject bedObject;

    // ─── Hint Key Label ──────────────────────────────────────────────────────
    [System.NonSerialized] public Text sleepHintText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        playerCtrl   = FindFirstObjectByType<PlayerController>();
        if (playerCtrl != null) originalSpeed = playerCtrl.moveSpeed;
        SpawnBed();
        CreateSleepHint();
    }

    void SpawnBed()
    {
        // Bed lives inside the interior room, slightly left of centre
        bedObject = new GameObject("Bed");
        bedObject.transform.position = new Vector3(-18f, -12f, 0f);

        SpriteRenderer sr = bedObject.AddComponent<SpriteRenderer>();
        sr.sprite       = bedSprite;
        sr.sortingOrder = 2;

        // Scale to roughly 1 tile
        bedObject.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        bedTransform = bedObject.transform;
    }

    void CreateSleepHint()
    {
        // Tiny world-space label above the bed shown when player is near
        sleepHintUI = new GameObject("SleepHint");
        Canvas wc = sleepHintUI.AddComponent<Canvas>();
        wc.renderMode       = RenderMode.WorldSpace;
        wc.sortingOrder     = 50;
        sleepHintUI.transform.localScale = Vector3.one * 0.012f;
        sleepHintUI.SetActive(false);

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(sleepHintUI.transform, false);
        sleepHintText = txtObj.AddComponent<Text>();
        sleepHintText.text      = "[E] Schlafen";
        sleepHintText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sleepHintText.fontSize  = 80;
        sleepHintText.alignment = TextAnchor.MiddleCenter;
        sleepHintText.color     = Color.white;

        ContentSizeFitter csf = txtObj.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
    }

    void Update()
    {
        if (isSleeping) return;

        // Passive tiredness gain
        currentTiredness = Mathf.Min(maxTiredness, currentTiredness + (tirednessPerMin / 60f) * Time.deltaTime);

        // Speed penalty when exhausted
        if (playerCtrl != null)
        {
            if (currentTiredness >= maxTiredness && !speedReduced)
            {
                playerCtrl.moveSpeed = originalSpeed * speedPenaltyPct;
                speedReduced = true;
                ShowExhaustionBanner();
            }
            else if (currentTiredness < maxTiredness * 0.95f && speedReduced)
            {
                playerCtrl.moveSpeed = originalSpeed;
                speedReduced = false;
            }
        }

        // Proximity hint & sleep interaction
        if (bedTransform != null && playerCtrl != null)
        {
            float dist = Vector2.Distance(playerCtrl.transform.position, bedTransform.position);
            bool near  = dist < bedInteractRadius;

            if (sleepHintUI != null)
            {
                sleepHintUI.SetActive(near);
                sleepHintUI.transform.position = bedTransform.position + new Vector3(0f, 0.3f, 0f);
            }

            if (near && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(SleepRoutine());
            }
        }

        UpdateHUD();
    }

    System.Collections.IEnumerator SleepRoutine()
    {
        isSleeping = true;
        if (sleepHintUI != null) sleepHintUI.SetActive(false);

        // Fade to black
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeToBlack(0.8f, null);

        yield return new WaitForSeconds(1.2f);

        // Reset tiredness
        currentTiredness = 0f;
        if (speedReduced && playerCtrl != null)
        {
            playerCtrl.moveSpeed = originalSpeed;
            speedReduced = false;
        }

        // Advance time of day by one third (roughly 8h)
        DayNightCycle dnc = FindFirstObjectByType<DayNightCycle>();
        if (dnc != null)
            dnc.timeOfDay = Mathf.Repeat(dnc.timeOfDay + 0.33f, 1f);

        yield return new WaitForSeconds(0.5f);

        // Fade back in
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeToClear(0.8f, null);

        yield return new WaitForSeconds(0.5f);

        isSleeping = false;

        // Show banner
        if (GameManager.Instance != null && GameManager.Instance.missionBannerManager != null)
            GameManager.Instance.missionBannerManager.ShowBanner(0, "Ausgeschlafen! Gute Energie für den Tag.");
    }

    void UpdateHUD()
    {
        if (tirednessBarFill == null) return;
        float pct = currentTiredness / maxTiredness;
        tirednessBarFill.localScale = new Vector3(pct, 1f, 1f);
        // colour: green -> yellow -> red
        Image img = tirednessBarFill.GetComponent<Image>();
        if (img != null)
            img.color = Color.Lerp(new Color(0.1f, 0.8f, 0.2f), new Color(0.9f, 0.2f, 0.1f), pct);
    }

    void ShowExhaustionBanner()
    {
        if (GameManager.Instance != null && GameManager.Instance.missionBannerManager != null)
            GameManager.Instance.missionBannerManager.ShowBanner(0, "😴 Zu müde! Geh schlafen, sonst wirst du langsamer.");
    }
}
