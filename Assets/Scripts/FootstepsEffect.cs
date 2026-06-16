using UnityEngine;
using System.Collections;

public class FootstepsEffect : MonoBehaviour
{
    public float stepDistance = 0.5f;
    private Vector3 lastStepPos;

    void Start()
    {
        lastStepPos = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastStepPos) >= stepDistance)
        {
            SpawnFootstep();
            lastStepPos = transform.position;
        }
    }

    private void SpawnFootstep()
    {
        GameObject dust = new GameObject("FootstepDust");
        dust.transform.position = transform.position + new Vector3(Random.Range(-0.1f, 0.1f), -0.2f, 0); // Adjusted height to be higher
        
        SpriteRenderer sr = dust.AddComponent<SpriteRenderer>();
        // Re-use an existing small white square/circle sprite or create a simple texture
        Texture2D tex = new Texture2D(8, 8);
        for(int x=0; x<8; x++) for(int y=0; y<8; y++) tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.6f));
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,8,8), new Vector2(0.5f, 0.5f), 100f);
        
        dust.transform.localScale = new Vector3(3f, 3f, 1f); // Make it big enough to see
        
        // Ground is usually 0, Props/Player are 2 (due to Y-sorting)
        // Set footsteps to 1 so they are on top of ground, but behind player/props
        // Map elements seem to use "Layer 1" based on PlayerController
        sr.sortingLayerName = "Layer 1";
        sr.sortingOrder = 1;

        StartCoroutine(FadeOutAndDestroy(sr, dust));
    }

    private IEnumerator FadeOutAndDestroy(SpriteRenderer sr, GameObject obj)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            obj.transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, 0) * 0.2f; // Expand slightly
            
            yield return null;
        }

        Destroy(obj);
    }
}
