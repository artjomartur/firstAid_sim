using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float sprintMultiplier = 1.8f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 18f;   // per second while sprinting
    public float staminaRegenRate = 12f;   // per second while not sprinting
    [System.NonSerialized] public float currentStamina = 100f;
    private bool sprintExhausted = false;  // locked out until >25%

    [Header("Dragging")]
    public float dragSpeedMultiplier = 0.55f;
    [System.NonSerialized] public bool isDraggingNPC = false;

    [Header("Miho Animators")]
    public Animator forwardAnimator;  // 0 - front facing (down/south)
    public Animator backwardAnimator; // 1 - back facing (up/north)
    
    // Original component references
    private Rigidbody2D rb;
    private Vector2 movement;
    
    // Animation constants matching AniManagers.cs
    private enum AniType { idle = 0, walk = 1, run = 2, win = 3, lose = 4 }
    private bool isFacingBackward = false;
    
    // Store original scale to preserve inspector settings (e.g., 0.1)
    private Vector3 baseScale;

    // Pose state to prevent overriding win/lose animations
    private bool isPose = false;
    private float dustTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-assign animators if they are missing
        if (forwardAnimator == null)
        {
            Transform fwd = transform.Find("miho_forward");
            if (fwd != null) forwardAnimator = fwd.GetComponent<Animator>();
        }
        if (backwardAnimator == null)
        {
            Transform bwd = transform.Find("miho_BACK");
            if (bwd != null) backwardAnimator = bwd.GetComponent<Animator>();
        }
        
        // DESTROY CONFLICTING ANIMATION SCRIPT
        Component aniManager = GetComponent("AniManagers");
        if (aniManager != null)
        {
            Destroy(aniManager);
        }

        // Ensure SortingGroup exists to prevent sprites from being sliced by Y-sorting environment
        UnityEngine.Rendering.SortingGroup sg = GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg == null)
        {
            sg = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
        }
        sg.sortingLayerName = "Layer 1";
        sg.sortingOrder = 2; // Match the sorting order of the props so Y-sorting works!

        // Fix Pivot so Sorting Group evaluates at feet instead of the face
        Transform shadow = transform.Find("shadow");
        if (shadow != null && shadow.localPosition.y > 1f)
        {
            float shiftY = shadow.localPosition.y; // Shift by shadow offset
            foreach (Transform child in transform)
            {
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y - shiftY, child.localPosition.z);
            }
            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                boxCol.offset = new Vector2(boxCol.offset.x, boxCol.offset.y - shiftY);
            }
            transform.position = new Vector3(transform.position.x, transform.position.y + (shiftY * transform.localScale.y), transform.position.z);
        }
        
#if UNITY_EDITOR
        // Auto-assign controllers if missing when hitting Play in the Editor!
        if (forwardAnimator != null && forwardAnimator.runtimeAnimatorController == null)
        {
            forwardAnimator.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/FW_MIHO/Animation/miho_forward_ANI.controller");
        }
        if (backwardAnimator != null && backwardAnimator.runtimeAnimatorController == null)
        {
            backwardAnimator.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/FW_MIHO/Animation/miho_BACK_ANI.controller");
        }
#endif
        
        UpdateAnimatorsState();
        
        // Grab the absolute scale from the inspector to maintain size
        baseScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));

        // Standard settings for 2D movement
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        // Initialize sorting layer for Miho so she renders above the ground (Layer 1)
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var childSr in srs)
        {
            childSr.sortingLayerName = "Layer 1";
            // Do NOT overwrite sortingOrder, it destroys the internal Z-order of the character's body parts!
        }
        
        UpdateAnimatorsState();
    }

    void Update()
    {
        // Allow testing the animations with 3 and 4, like in the old script
        if (Input.GetKeyDown(KeyCode.Alpha3)) TriggerWin();
        if (Input.GetKeyDown(KeyCode.Alpha4)) TriggerLose();
        
        // Reset pose when moving
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || 
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || 
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || 
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            isPose = false;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Determine if running
        bool isRun = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Update direction and flip
        if (movement.magnitude > 0)
        {
            isPose = false; // Break out of poses when moving

            if (movement.y > 0) // Moving North/Up -> Back facing
            {
                isFacingBackward = true;
            }
            else if (movement.y < 0) // Moving South/Down -> Front facing
            {
                isFacingBackward = false;
            }

            // Flip X scale depending on horizontal movement
            // In Miho's original script: x=-1 scale is right facing, x=1 is left facing
            if (movement.x > 0)
            {
                transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
            }
            else if (movement.x < 0)
            {
                transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
            }
            
            UpdateAnimatorsState();

            // Set animation state if not in a pose
            if (!isPose)
            {
                AniType state = isRun ? AniType.run : AniType.walk;
                SetAniState(state);
            }
        }
        else if (!isPose)
        {
            // Idle
            SetAniState(AniType.idle);
        }
    }

    void FixedUpdate()
    {
        bool wantSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isMoving   = movement.sqrMagnitude > 0.01f;

        // Stamina drain / regen
        if (wantSprint && isMoving && !sprintExhausted)
        {
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.fixedDeltaTime);
            if (currentStamina <= 0) sprintExhausted = true;
        }
        else
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.fixedDeltaTime);
            if (sprintExhausted && currentStamina >= maxStamina * 0.25f)
                sprintExhausted = false;
        }

        float currentSpeed = moveSpeed;
        if (wantSprint && isMoving && !sprintExhausted)
        {
            currentSpeed *= sprintMultiplier;
        }
        if (isDraggingNPC)
        {
            currentSpeed *= dragSpeedMultiplier;
        }

        if (isMoving)
        {
            dustTimer -= Time.fixedDeltaTime;
            if (dustTimer <= 0f)
            {
                SpawnDustParticle();
                dustTimer = (wantSprint && !sprintExhausted) ? 0.16f : 0.32f;
            }
        }

        if (rb != null)
            rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
        else
            transform.Translate(movement.normalized * currentSpeed * Time.deltaTime);
    }

    private void UpdateAnimatorsState()
    {
        if (forwardAnimator != null && backwardAnimator != null)
        {
            if (isFacingBackward)
            {
                forwardAnimator.gameObject.SetActive(false);
                backwardAnimator.gameObject.SetActive(true);
            }
            else
            {
                forwardAnimator.gameObject.SetActive(true);
                backwardAnimator.gameObject.SetActive(false);
            }
        }
    }

    private void SetAniState(AniType type)
    {
        Animator activeAnim = isFacingBackward ? backwardAnimator : forwardAnimator;
        if (activeAnim != null && activeAnim.gameObject.activeInHierarchy)
        {
            activeAnim.SetInteger("aniInt", (int)type);
        }
    }
    
    // Optional methods to trigger win/lose from GameManager
    public void TriggerWin()
    {
        isPose = true;
        SetAniState(AniType.win);
    }
    
    public void TriggerLose()
    {
        isPose = true;
        SetAniState(AniType.lose);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-find children
        if (forwardAnimator == null)
        {
            Transform fwd = transform.Find("miho_forward");
            if (fwd != null) forwardAnimator = fwd.GetComponent<Animator>();
        }
        if (backwardAnimator == null)
        {
            Transform bwd = transform.Find("miho_BACK");
            if (bwd != null) backwardAnimator = bwd.GetComponent<Animator>();
        }

        // Auto-assign controllers if missing
        if (forwardAnimator != null && forwardAnimator.runtimeAnimatorController == null)
        {
            forwardAnimator.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/FW_MIHO/Animation/miho_forward_ANI.controller");
        }
        if (backwardAnimator != null && backwardAnimator.runtimeAnimatorController == null)
        {
            backwardAnimator.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/FW_MIHO/Animation/miho_BACK_ANI.controller");
        }

        // Automatically add SortingGroup in the Editor so she isn't sliced by the environment
        UnityEngine.Rendering.SortingGroup sg = GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg == null)
        {
            sg = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
        }
        if (sg.sortingLayerName != "Layer 1")
        {
            sg.sortingLayerName = "Layer 1";
        }

        // Automatically fix the sorting layer in the Editor for children just in case
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);
        if (srs != null)
        {
            foreach (var childSr in srs)
            {
                if (childSr.sortingLayerName != "Layer 1")
                {
                    childSr.sortingLayerName = "Layer 1";
                    // Do NOT overwrite sortingOrder here either
                }
            }
        }
    }
#endif

    private static Sprite _dustSprite;
    private static Sprite GetDustSprite()
    {
        if (_dustSprite != null) return _dustSprite;
        
        Texture2D tex = new Texture2D(16, 16);
        Color[] cols = new Color[16 * 16];
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f;
                float dy = y - 7.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= 7.5f)
                {
                    // Soft circular fade out at the edges
                    float alpha = Mathf.Clamp01(1f - (dist / 7.5f));
                    cols[y * 16 + x] = new Color(1f, 1.0f, 1f, alpha);
                }
                else
                {
                    cols[y * 16 + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        _dustSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        return _dustSprite;
    }

    private void SpawnDustParticle()
    {
        GameObject dust = new GameObject("FootstepDust");
        dust.transform.position = transform.position + new Vector3(0f, -0.6f, 0f);
        
        SpriteRenderer sr = dust.AddComponent<SpriteRenderer>();
        sr.sprite = GetDustSprite();
        sr.color = new Color(0.9f, 0.9f, 0.9f, 0.4f); // Soft grey-white dust
        sr.sortingLayerName = "Layer 1";
        sr.sortingOrder = 1; // Render above ground tiles but below character parts

        dust.AddComponent<DustPuff>();
    }
}

public class DustPuff : MonoBehaviour
{
    private float life = 0.5f;
    private Vector3 dir;
    private SpriteRenderer sr;

    private void Start()
    {
        dir = new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.05f, 0.15f), 0f);
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * Random.Range(0.06f, 0.11f);
    }

    private void Update()
    {
        transform.position += dir * Time.deltaTime;
        transform.localScale -= Vector3.one * Time.deltaTime * 0.12f;
        life -= Time.deltaTime;
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, life * 2f);
        }
        if (life <= 0f) Destroy(gameObject);
    }
}
