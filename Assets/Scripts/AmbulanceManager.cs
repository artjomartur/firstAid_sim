using UnityEngine;
using System;
using System.Collections;

public class AmbulanceManager : MonoBehaviour
{
    public Transform ambulanceTransform;
    public SpriteRenderer ambulanceRenderer;
    public Sprite ambulanceSprite;
    
    private bool isFlashing = false;

    public void SpawnAmbulance(Vector3 victimPos, Action onComplete)
    {
        gameObject.SetActive(true);
        isFlashing = true;
        StartCoroutine(AmbulanceRoutine(victimPos, onComplete));
        StartCoroutine(FlashLights());
    }

    private IEnumerator FlashLights()
    {
        while (isFlashing)
        {
            if (ambulanceRenderer != null)
                ambulanceRenderer.color = (ambulanceRenderer.color == Color.white) ? new Color(0.5f, 0.5f, 1f) : Color.white;
            yield return new WaitForSeconds(0.15f);
        }
        if (ambulanceRenderer != null) ambulanceRenderer.color = Color.white;
    }

    private IEnumerator AmbulanceRoutine(Vector3 victimPos, Action onComplete)
    {
        ambulanceTransform.gameObject.SetActive(true);
        Vector3 startPos = victimPos + new Vector3(20, 0, 0); // Start off-screen (world units)
        ambulanceTransform.position = startPos;

        // Drive to victim
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.7f;
            ambulanceTransform.position = Vector3.Lerp(startPos, victimPos + new Vector3(2, 0, 0), t);
            yield return null;
        }

        yield return new WaitForSeconds(2.5f); // Wait at victim

        // Drive off with victim
        Vector3 endPos = victimPos + new Vector3(-30, 0, 0);
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.7f;
            ambulanceTransform.position = Vector3.Lerp(victimPos + new Vector3(2, 0, 0), endPos, t);
            yield return null;
        }

        isFlashing = false;
        ambulanceTransform.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}
