using UnityEngine;
using System.Collections;
using FreewrokGame; // to access AniManagers

public class MihoNPC : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float runSpeed = 3f;
    public float wanderRadius = 5f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 6f;

    private Animator[] targetAnimators;
    private Vector3 startPos;
    private Vector3 targetPos;
    private int currentDirection = 0; // 0 = forward, 1 = backward

    private string[] chatterLines = new string[] 
    {
        "Schönes Wetter heute!",
        "Hier ist es so friedlich.",
        "Hoffentlich passiert nichts...",
        "Ich liebe diesen Park.",
        "Muss gleich noch einkaufen.",
        "Was für ein schöner Tag!"
    };

    private Coroutine wanderCo;
    private Coroutine chatterCo;

    void Awake()
    {
        // Get animators from the original manager and destroy it so it doesn't take input
        AniManagers originalManager = GetComponent<AniManagers>();
        if (originalManager != null)
        {
            targetAnimators = originalManager.targetAnimators;
            Destroy(originalManager);
        }

        startPos = transform.position;
    }

    void OnEnable()
    {
        if (startPos == Vector3.zero) startPos = transform.position;
        wanderCo = StartCoroutine(WanderRoutine());
        chatterCo = StartCoroutine(ChatterRoutine());
    }

    void OnDisable()
    {
        if (wanderCo != null) { StopCoroutine(wanderCo); wanderCo = null; }
        if (chatterCo != null) { StopCoroutine(chatterCo); chatterCo = null; }

        // Clean up chatter canvases to prevent "burned in" boxes
        foreach (Transform child in transform)
        {
            if (child.name == "ChatterCanvas" || child.name == "ChatterCanvas_Prompt" || child.name == "ChatterCanvas_Speech")
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void SetAnimation(int aniInt)
    {
        if (targetAnimators == null || targetAnimators.Length < 2) return;

        if (!targetAnimators[currentDirection].gameObject.activeSelf)
        {
            targetAnimators[currentDirection].gameObject.SetActive(true);
            int otherDir = currentDirection == 0 ? 1 : 0;
            targetAnimators[otherDir].gameObject.SetActive(false);
        }

        targetAnimators[currentDirection].SetInteger("aniInt", aniInt);
    }

    IEnumerator ChatterRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8f, 20f));

            if (Random.value > 0.5f)
            {
                ShowChatter(chatterLines[Random.Range(0, chatterLines.Length)]);
            }
        }
    }

    private void ShowChatter(string text)
    {
        // Destroy existing chatter if any
        foreach (Transform child in transform) { if (child.name == "ChatterCanvas") Destroy(child.gameObject); }

        GameObject canvasObj = new GameObject("ChatterCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, 1.8f, 0); // Above head
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingLayerName = "Layer 1";
        canvas.sortingOrder = 300; // Above player
        
        RectTransform rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 60);
        rt.localScale = new Vector3(0.05f, 0.05f, 1f);
        
        // Windows block background
        GameObject bgObj = new GameObject("Bg");
        bgObj.transform.SetParent(canvasObj.transform, false);
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
        outline.effectDistance = new Vector2(2f, -2f);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = 24;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.black;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        
        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(5, 5); // padding
        txtRt.offsetMax = new Vector2(-5, -5);

        Destroy(canvasObj, 4f);
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Idle
            SetAnimation(0); // 0 = idle
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            // Pick a new target
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            targetPos = startPos + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Decide to walk or run
            bool isRunning = Random.value > 0.7f;
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            
            // Determine direction (up/down and left/right)
            float dirX = targetPos.x - transform.position.x;
            float dirY = targetPos.y - transform.position.y;
            
            // If moving mostly up, use backward animation
            currentDirection = dirY > 0 ? 1 : 0;

            SetAnimation(isRunning ? 2 : 1);

            // Flip based on horizontal direction
            if (Mathf.Abs(dirX) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = dirX < 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
                
                // Keep chatter upright even if we flip
                foreach (Transform child in transform)
                {
                    if (child.name == "ChatterCanvas")
                    {
                        Vector3 cScale = child.localScale;
                        cScale.x = Mathf.Sign(scale.x) * 0.01f;
                        child.localScale = cScale;
                    }
                }
            }

            // Move
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = false;
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            // Also ensure it has a collider
            if (GetComponent<Collider2D>() == null)
            {
                CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
                col.radius = 4f; // Larger radius to match the scale of the sprite
                col.offset = new Vector2(0, 2f); // Offset slightly upwards to cover base body
            }

            Vector2 lastPos = rb.position;
            float stuckTimer = 0f;

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, currentSpeed * Time.deltaTime);
                rb.MovePosition(newPos);
                yield return new WaitForFixedUpdate();

                // Check if we are stuck pushing against a collider/wall
                if (Vector2.Distance(rb.position, lastPos) < 0.01f)
                {
                    stuckTimer += Time.fixedDeltaTime;
                    if (stuckTimer > 0.6f) // Stuck for more than 0.6s, abort and wait/pick a new target
                    {
                        break;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }
                lastPos = rb.position;
            }
        }
    }
}
