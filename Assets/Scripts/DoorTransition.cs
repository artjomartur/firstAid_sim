using UnityEngine;
using System.Collections;

public class DoorTransition : MonoBehaviour
{
    [Header("Settings")]
    public Transform targetPosition;
    public bool requireInteraction = true;
    public float fadeDuration = 0.5f;
    
    [Header("Environment")]
    [Tooltip("If true, removes the Day/Night filter when teleporting here.")]
    public bool targetIsIndoors = true;

    [Header("Game Feel")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    public bool useCloudTransition = false;
    
    [Header("Interaction Prompt")]
    public GameObject interactionPromptPrefab;
    public float promptYOffset = 1.5f;
    private GameObject currentPrompt;
    
    private bool isPlayerNear = false;
    private PlayerController playerCtrl;
    private bool isTransitioning = false;
    private GameObject interactionPrompt;
    private static float lastTeleportTime = 0f;

    private void Awake()
    {
        // Fix: If the object accidentally has a 3D BoxCollider instead of a 2D one, replace it at runtime!
        BoxCollider box3D = GetComponent<BoxCollider>();
        if (box3D != null)
        {
            BoxCollider2D box2D = gameObject.AddComponent<BoxCollider2D>();
            box2D.isTrigger = box3D.isTrigger;
            box2D.offset = box3D.center;
            box2D.size = box3D.size;
            Destroy(box3D);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("DoorTransition: Trigger entered by " + collision.gameObject.name + " with tag " + collision.gameObject.tag);
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            isPlayerNear = true;
            playerCtrl = collision.GetComponent<PlayerController>();
            
            if (requireInteraction && currentPrompt == null && !isTransitioning)
            {
                ShowPrompt();
            }
        }
    }

    private void ShowPrompt()
    {
        if (interactionPromptPrefab != null)
        {
            currentPrompt = Instantiate(interactionPromptPrefab, transform.position + Vector3.up * promptYOffset, Quaternion.identity);
            return;
        }
    }

    void OnGUI()
    {
        if (isPlayerNear && requireInteraction && !isTransitioning)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * promptYOffset);
            if (screenPos.z > 0)
            {
                screenPos.y = Screen.height - screenPos.y;
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 20;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.yellow;
                GUI.Box(new Rect(screenPos.x - 75, screenPos.y - 25, 150, 50), "[ E ] Eintreten", style);
            }
        }
    }

    private void GeneratePromptFallback()
    {
        currentPrompt = new GameObject("InteractionPrompt");
        currentPrompt.transform.position = transform.position + Vector3.up * promptYOffset;

        GameObject canvasObj = new GameObject("PromptCanvas");
        canvasObj.transform.SetParent(currentPrompt.transform);
        canvasObj.transform.localPosition = Vector3.zero;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;
        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(180, 50);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 1f);

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        // Using a solid color rectangle instead of a sprite to avoid missing resource errors
        bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        bgImg.rectTransform.sizeDelta = new Vector2(180, 50);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bgObj.transform, false);
        UnityEngine.UI.Text txt = textObj.AddComponent<UnityEngine.UI.Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "[ E ] Eintreten";
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(1f, 0.85f, 0.3f); // Golden yellow text
        txt.rectTransform.sizeDelta = new Vector2(180, 50);

        // Add a simple floating animation
        currentPrompt.AddComponent<FloatingPrompt>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            isPlayerNear = true;
            if (playerCtrl == null) playerCtrl = collision.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (currentPrompt != null)
            {
                Destroy(currentPrompt);
                currentPrompt = null;
            }
        }
    }

    private void Update()
    {
        if (isPlayerNear && !isTransitioning)
        {
            if (requireInteraction)
            {
                if (Input.GetKeyDown(KeyCode.E)) StartTransition();
            }
            else
            {
                if (Time.time - lastTeleportTime > 1.0f) StartTransition();
            }
        }
    }

    private void StartTransition()
    {
        if (targetPosition == null)
        {
            Debug.LogWarning("DoorTransition: No target position set!");
            return;
        }

        isTransitioning = true;
        lastTeleportTime = Time.time;
        
        // Disable player movement during transition
        if (playerCtrl != null)
        {
            playerCtrl.enabled = false;
            Rigidbody2D rb = playerCtrl.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (currentPrompt != null) 
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }

        AudioClip openSnd = doorOpenSound != null ? doorOpenSound : (AudioManager.Instance != null ? AudioManager.Instance.doorOpenSound : null);
        if (AudioManager.Instance != null && openSnd != null)
        {
            AudioManager.Instance.PlaySFX(openSnd);
        }

        if (ScreenFader.Instance != null)
        {
            if (useCloudTransition)
            {
                ScreenFader.Instance.CloudTransition(fadeDuration, OnFadeComplete);
            }
            else
            {
                ScreenFader.Instance.FadeToBlack(fadeDuration, OnFadeComplete);
            }
        }
        else
        {
            OnFadeComplete();
        }
    }

    private void OnFadeComplete()
    {
        AudioClip closeSnd = doorCloseSound != null ? doorCloseSound : (AudioManager.Instance != null ? AudioManager.Instance.doorCloseSound : null);
        if (AudioManager.Instance != null && closeSnd != null)
        {
            AudioManager.Instance.PlaySFX(closeSnd);
        }

        // Teleport the player
        if (playerCtrl != null)
        {
            playerCtrl.transform.position = targetPosition.position;
            
            // Re-enable movement
            playerCtrl.enabled = true;

            // Snap the camera immediately so it doesn't pan across the map
            Vector3 newCamPos = targetPosition.position;
            newCamPos.z = Camera.main.transform.position.z;
            Camera.main.transform.position = newCamPos;

            DayNightCycle dnc = Camera.main.GetComponent<DayNightCycle>();
            if (dnc != null)
            {
                dnc.isIndoors = targetIsIndoors;
            }

            // Show/hide the sky background when transitioning indoors/outdoors
            GameObject bgObj = GameObject.Find("Background");
            if (bgObj != null)
            {
                bgObj.SetActive(!targetIsIndoors);
            }
        }

        // Fade back in
        if (ScreenFader.Instance != null && !useCloudTransition)
        {
            ScreenFader.Instance.FadeToClear(fadeDuration, () => {
                isTransitioning = false;
            });
        }
        else
        {
            if (useCloudTransition)
            {
                StartCoroutine(EndTransitionDelay());
            }
            else
            {
                isTransitioning = false;
            }
        }
    }
    
    private IEnumerator EndTransitionDelay()
    {
        yield return new WaitForSeconds(fadeDuration / 2f);
        isTransitioning = false;
    }
}

public class FloatingPrompt : MonoBehaviour
{
    private Vector3 startPos;
    void Start() { startPos = transform.position; }
    void Update() { transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * 4f) * 0.1f, 0); }
}
