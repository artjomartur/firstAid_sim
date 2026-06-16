using UnityEngine;

public class NPCWander : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float runSpeed = 2.5f;
    public float wanderRadius = 3f;
    public float waitTime = 3f;
    public float reactionRadius = 15f; // Distance to react to accidents

    private Vector3 startPos;
    private Vector3 targetPos;
    private float waitTimer;
    private SpriteRenderer sr;
    private Animator[] animators;

    public bool isPanicking = false;
    private GameManager gm;

    [Header("Physical Dragging")]
    public bool isBeingDragged = false;
    public bool isCleared = false;
    private GameObject clearedVictim;
    private GameObject chatterCanvas;
    private MihoNPC mihoNPC;

    void Start()
    {
        startPos = transform.position;
        sr = GetComponentInChildren<SpriteRenderer>();
        animators = GetComponentsInChildren<Animator>();
        gm = FindFirstObjectByType<GameManager>();
        mihoNPC = GetComponent<MihoNPC>();
        PickNewTarget();
    }

    private GameObject GetCurrentVictim()
    {
        if (gm == null || gm.availableMissions == null) return null;
        foreach (var m in gm.availableMissions)
        {
            if (m.activeInstance != null && m.hasGaffers) return m.activeInstance;
        }
        return null;
    }

    void Update()
    {
        // 1. Check for accident
        GameObject victim = GetCurrentVictim();
        if (victim != null)
        {
            if (clearedVictim != victim)
            {
                isCleared = false;
                clearedVictim = victim;
            }

            if (isCleared)
            {
                if (mihoNPC != null && !mihoNPC.enabled) mihoNPC.enabled = true;
                if (mihoNPC == null) NormalWander();
                return;
            }

            // Disable MihoNPC during active accident reaction to prevent movement/chatter conflict
            if (mihoNPC != null && mihoNPC.enabled) mihoNPC.enabled = false;

            PlayerController playerCtrl = Object.FindAnyObjectByType<PlayerController>();
            GameObject player = playerCtrl != null ? playerCtrl.gameObject : null;

            if (isBeingDragged)
            {
                // Check if player still holds E
                if (player == null || !Input.GetKey(KeyCode.E))
                {
                    isBeingDragged = false;
                    if (playerCtrl != null) playerCtrl.isDraggingNPC = false;
                }
                else
                {
                    // NPC follows player
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, runSpeed * Time.deltaTime);
                    SetAnimation(2); // 2 = Run / Follow
                    FlipSprite(player.transform.position.x - transform.position.x);

                    // Check distance to victim
                    if (Vector3.Distance(transform.position, victim.transform.position) > 2.0f)
                    {
                        isCleared = true;
                        isBeingDragged = false;
                        isPanicking = false;
                        if (playerCtrl != null) playerCtrl.isDraggingNPC = false;

                        // Update wandering bounds around new position
                        startPos = transform.position;
                        PickNewTarget();

                        // Play success audio
                        if (AudioManager.Instance != null && AudioManager.Instance.uiClickSound != null)
                        {
                            AudioManager.Instance.PlaySFX(AudioManager.Instance.uiClickSound);
                        }

                        // Display speech bubble
                        ShowChatter("Okay, ich halte Abstand!", 3f);
                    }
                    return; // Skip other behaviors while being dragged
                }
            }

            float distToVictim = Vector3.Distance(transform.position, victim.transform.position);
            if (distToVictim < reactionRadius)
            {
                if (!isPanicking)
                {
                    isPanicking = true;
                    waitTimer = 0f;
                }

                // Check if player is close enough to drag
                if (player != null)
                {
                    float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
                    if (distToPlayer < 1.8f && !playerCtrl.isDraggingNPC)
                    {
                        // Player is close, show prompt
                        if (chatterCanvas == null)
                        {
                            ShowChatter("[E] Wegziehen", 0f);
                        }

                        if (Input.GetKey(KeyCode.E))
                        {
                            isBeingDragged = true;
                            playerCtrl.isDraggingNPC = true;
                            if (chatterCanvas != null)
                            {
                                Destroy(chatterCanvas);
                                chatterCanvas = null;
                            }
                            return; // Start dragging immediately
                        }
                    }
                    else
                    {
                        // Player is far or another NPC is being dragged, clear prompt
                        if (chatterCanvas != null && chatterCanvas.name == "ChatterCanvas_Prompt")
                        {
                            Destroy(chatterCanvas);
                            chatterCanvas = null;
                        }
                    }
                }

                if (distToVictim > 1.5f)
                {
                    // Run to victim
                    transform.position = Vector3.MoveTowards(transform.position, victim.transform.position, runSpeed * Time.deltaTime);
                    SetAnimation(2); // 2 = Run
                    FlipSprite(victim.transform.position.x - transform.position.x);
                }
                else
                {
                    // Arrived, panic!
                    SetAnimation(4); // 4 = Lose/Panic
                    FlipSprite(victim.transform.position.x - transform.position.x);
                }
                return; // Skip normal wander
            }
        }
        else
        {
            if (isPanicking)
            {
                isPanicking = false;
                PickNewTarget(); // Go back to normal
            }
            if (chatterCanvas != null)
            {
                Destroy(chatterCanvas);
                chatterCanvas = null;
            }
        }

        // 2. Normal Wander
        if (mihoNPC != null && !mihoNPC.enabled) mihoNPC.enabled = true;
        if (mihoNPC == null) NormalWander();
    }

    private void NormalWander()
    {
        if (isPanicking)
        {
            isPanicking = false;
            PickNewTarget();
        }

        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            SetAnimation(0); // 0 = Idle
            return;
        }

        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        SetAnimation(1); // 1 = Walk
        
        float dirX = targetPos.x - transform.position.x;
        FlipSprite(dirX);

        // Pick new target when arrived
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            waitTimer = Random.Range(waitTime * 0.5f, waitTime * 1.5f);
            PickNewTarget();
        }
    }

    private void ShowChatter(string text, float duration = 4f)
    {
        // Destroy existing chatter if any
        if (chatterCanvas != null)
        {
            Destroy(chatterCanvas);
            chatterCanvas = null;
        }

        chatterCanvas = new GameObject(duration == 0f ? "ChatterCanvas_Prompt" : "ChatterCanvas_Speech");
        chatterCanvas.transform.SetParent(transform);
        chatterCanvas.transform.localPosition = new Vector3(0, 1.8f, 0); // Above head
        
        Canvas canvas = chatterCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingLayerName = "Layer 1";
        canvas.sortingOrder = 300; // Above player
        
        RectTransform rt = chatterCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
        rt.localScale = new Vector3(0.008f, 0.008f, 1f);
        
        // Windows block background
        GameObject bgObj = new GameObject("Bg");
        bgObj.transform.SetParent(chatterCanvas.transform, false);
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.85f, 0.85f, 0.85f, 1f); // Light grey Windows 95
        
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Add a thin black border
        UnityEngine.UI.Outline outline = bgObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(chatterCanvas.transform, false);
        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = 28;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.black;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        
        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(8, 8); // padding
        txtRt.offsetMax = new Vector2(-8, -8);

        if (duration > 0f)
        {
            Destroy(chatterCanvas, duration);
        }
    }

    private void LateUpdate()
    {
        if (chatterCanvas != null)
        {
            Vector3 parentScale = transform.localScale;
            float px = parentScale.x;
            float py = parentScale.y;

            if (Mathf.Abs(px) < 0.001f) px = 1f;
            if (Mathf.Abs(py) < 0.001f) py = 1f;

            // Position above head in world space
            chatterCanvas.transform.localPosition = new Vector3(0, 1.8f / Mathf.Abs(py), 0);

            // Calculate localScale to maintain constant world scale of 0.008f without mirroring
            float desiredScale = 0.008f;
            float localScaleX = desiredScale / Mathf.Abs(px);
            if (parentScale.x < 0) localScaleX = -localScaleX;

            float localScaleY = desiredScale / Mathf.Abs(py);
            if (parentScale.y < 0) localScaleY = -localScaleY;

            chatterCanvas.transform.localScale = new Vector3(localScaleX, localScaleY, 1f);
        }
    }

    private void FlipSprite(float dirX)
    {
        if (Mathf.Abs(dirX) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            // Match PlayerController: positive dirX (right) means negative scale.x
            scale.x = dirX > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void PickNewTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        targetPos = startPos + new Vector3(randomOffset.x, randomOffset.y, 0);
    }

    private void SetAnimation(int aniInt)
    {
        if (animators == null || animators.Length == 0) return;

        foreach (var anim in animators)
        {
            if (anim == null) continue;

            // Try Cainos parameter
            bool hasIsMoving = false;
            foreach (var p in anim.parameters) { if (p.name == "IsMoving") hasIsMoving = true; }
            if (hasIsMoving)
            {
                anim.SetBool("IsMoving", aniInt == 1 || aniInt == 2);
                continue; // Skip Miho logic if Cainos
            }

            // Try Miho parameter
            bool hasAniInt = false;
            foreach (var p in anim.parameters) { if (p.name == "aniInt") hasAniInt = true; }
            if (hasAniInt)
            {
                anim.SetInteger("aniInt", aniInt);
            }
        }
    }

    private void OnDisable()
    {
        if (chatterCanvas != null) Destroy(chatterCanvas);
    }

    private void OnDestroy()
    {
        if (chatterCanvas != null) Destroy(chatterCanvas);
    }
}
