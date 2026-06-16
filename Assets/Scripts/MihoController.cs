using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MihoController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float sprintMultiplier = 1.8f;

    [Header("Miho Animators (0=Front, 1=Back)")]
    public Animator[] targetAnimators;

    private Rigidbody2D rb;
    private Vector2 movement;
    private int currentDirType = 0; // 0 = forward, 1 = backward
    private bool isPose;
    private AniState poseState = AniState.Idle;
    private readonly List<SpriteRenderer> hairStrandRenderers = new List<SpriteRenderer>();
    private readonly HashSet<SpriteRenderer> permanentlyHiddenHair = new HashSet<SpriteRenderer>();

    // Animation States matching Miho's setup
    private enum AniState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Win = 3,
        Lose = 4
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        // Physics-Layer wie Cainos-Top-Down (kollidiert mit Wänden auf Layer 2/3)
        int layer1 = LayerMask.NameToLayer("Layer 1");
        if (layer1 >= 0)
            gameObject.layer = layer1;

        // Sorting: über Tilemaps (Layer 3), nicht hinter der Map
        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup != null)
        {
            sortingGroup.sortingLayerName = "Layer 3";
            sortingGroup.sortingOrder = 50;
        }

        // Collider automatisch anpassen (der Standard-Collider vom Prefab ist oft viel zu groß)
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            float s = Mathf.Abs(transform.localScale.x);
            if (s > 0)
            {
                box.size = new Vector2(0.1f / s, 0.08f / s);
                box.offset = Vector2.zero;
            }
        }

        // Finde die Animatoren automatisch, falls nicht zugewiesen oder leer
        bool needsAnimators = targetAnimators == null || targetAnimators.Length < 2 || targetAnimators[0] == null || targetAnimators[1] == null;
        
        if (needsAnimators)
        {
            Animator[] foundAnimators = GetComponentsInChildren<Animator>(true);
            System.Collections.Generic.List<Animator> childAnimators = new System.Collections.Generic.List<Animator>();
            
            // Nur Animatoren nehmen, die NICHT auf dem Haupt-Objekt liegen
            foreach (var anim in foundAnimators)
            {
                if (anim.gameObject != this.gameObject)
                {
                    childAnimators.Add(anim);
                }
            }

            if (childAnimators.Count >= 2)
            {
                targetAnimators = new Animator[2];
                // Wir gehen davon aus, dass miho_forward zuerst kommt oder wir weisen sie anhand des Namens zu
                foreach(var a in childAnimators)
                {
                    if (a.name.ToLower().Contains("back")) targetAnimators[1] = a;
                    else targetAnimators[0] = a; // Fallback to forward
                }
                Debug.Log("[MihoController] Animatoren automatisch gefunden: " + targetAnimators[0].name + " & " + targetAnimators[1].name);
            }
            else
            {
                Debug.LogWarning("[MihoController] Konnte nicht beide Animatoren (Front/Back) finden!");
            }
        }

        RemoveFlyingHair();
    }

    /// <summary>
    /// Entfernt lose Haar-Layer und Haar-Knochen, die nicht am Skelett hängen (fliegende Haare).
    /// </summary>
    private void RemoveFlyingHair()
    {
        hairStrandRenderers.Clear();
        permanentlyHiddenHair.Clear();

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            string name = t.name;

            // Duplikat-Haar-Layer (große lose Sprites)
            if (name == "F_B_HAIR" || name == "B_HAIR")
            {
                t.gameObject.SetActive(false);
                continue;
            }

            if (!name.Contains("HAIR_bone")) continue;

            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (!IsHairAttachedToSkeleton(t))
            {
                if (sr != null)
                {
                    sr.enabled = false;
                    permanentlyHiddenHair.Add(sr);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
                continue;
            }

            if (sr != null && !hairStrandRenderers.Contains(sr))
                hairStrandRenderers.Add(sr);
        }
    }

    private static bool IsHairAttachedToSkeleton(Transform hairBone)
    {
        Transform p = hairBone.parent;
        while (p != null)
        {
            string n = p.name;
            if (n.Contains("HEAD_bone") || n.Contains("F_B_bone") || n.Contains("B_B_bone"))
                return true;
            p = p.parent;
        }
        return false;
    }

    private void SetHairStrandsVisible(bool visible)
    {
        foreach (SpriteRenderer sr in hairStrandRenderers)
        {
            if (sr != null && !permanentlyHiddenHair.Contains(sr))
                sr.enabled = visible;
        }
    }

    void Update()
    {
        // Test-Tasten wie im FW_MIHO Demo (3 = Win, 4 = Lose)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayWinAnimation();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayLoseAnimation();
            return;
        }

        // 1. Input lesen (WASD / Pfeiltasten)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        bool isMoving = movement.sqrMagnitude > 0;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Bewegung beendet Pose-Animation
        if (isPose && isMoving)
        {
            isPose = false;
        }

        // 2. Richtung und Skalierung (Spiegelung) anpassen
        if (isMoving)
        {
            if (movement.y > 0)
            {
                currentDirType = 1;
            }
            else if (movement.y < 0)
            {
                currentDirType = 0;
            }

            float currentScaleSize = Mathf.Abs(transform.localScale.y);

            if (movement.x > 0)
            {
                transform.localScale = new Vector3(-currentScaleSize, currentScaleSize, currentScaleSize);
            }
            else if (movement.x < 0)
            {
                transform.localScale = new Vector3(currentScaleSize, currentScaleSize, currentScaleSize);
            }
        }

        // 3. Animation setzen
        if (isPose)
        {
            SetAnimation(poseState);
            return;
        }

        AniState currentState = AniState.Idle;
        if (isMoving)
        {
            currentState = isSprinting ? AniState.Run : AniState.Walk;
        }

        SetAnimation(currentState);
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
        }
    }

    private void SetAnimation(AniState state)
    {
        if (targetAnimators == null || targetAnimators.Length < 2) return;

        // Richtigen Animator aktivieren und den anderen deaktivieren
        for (int i = 0; i < targetAnimators.Length; i++)
        {
            if (targetAnimators[i] != null)
            {
                bool shouldBeActive = (i == currentDirType);
                if (targetAnimators[i].gameObject.activeSelf != shouldBeActive)
                {
                    targetAnimators[i].gameObject.SetActive(shouldBeActive);
                }
                
                // Setze den Animations-Parameter "aniInt" (0=Idle, 1=Walk, 2=Run)
                if (shouldBeActive)
                {
                    targetAnimators[i].SetInteger("aniInt", (int)state);
                }
            }
        }

        // Win/Lose-Animationen bewegen Haar-Knochen falsch → Stränge ausblenden
        bool hideHairStrands = state == AniState.Win || state == AniState.Lose;
        SetHairStrandsVisible(!hideHairStrands);
    }

    /// <summary>
    /// Spielt die Win-Animation ab (z.B. nach erfolgreicher Mission)
    /// </summary>
    public void PlayWinAnimation()
    {
        isPose = true;
        poseState = AniState.Win;
        currentDirType = 0;
        SetAnimation(AniState.Win);
    }

    /// <summary>
    /// Spielt die Lose-Animation ab (z.B. bei fehlgeschlagener Mission)
    /// </summary>
    public void PlayLoseAnimation()
    {
        isPose = true;
        poseState = AniState.Lose;
        currentDirType = 0;
        SetAnimation(AniState.Lose);
    }

    public void ClearPoseAnimation()
    {
        isPose = false;
        SetAnimation(AniState.Idle);
    }
}
