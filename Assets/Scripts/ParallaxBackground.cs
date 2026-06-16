using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("0 = bewegt sich mit der Kamera, 1 = bewegt sich gar nicht (statisch im Hintergrund)")]
    public float parallaxEffectX = 0.8f;
    public float parallaxEffectY = 0.8f;
    
    [Header("Repeat/Loop")]
    public bool repeatX = true;
    public bool repeatY = true;

    private GameObject cam;
    private float lengthX;
    private float lengthY;
    private float startposX;
    private float startposY;

    void Start()
    {
        FindCamera();
        
        if (cam != null)
        {
            // Automatisch auf die Kamera zentrieren, egal wo der User es im Scene-View hinzieht!
            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
        }
        
        startposX = transform.position.x;
        startposY = transform.position.y;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            lengthX = sr.bounds.size.x;
            lengthY = sr.bounds.size.y;
        }

        // Auto-create clones for seamless looping only if length is valid!
        if (repeatX && lengthX > 0)
        {
            CreateClone(new Vector3(lengthX, 0, 0));
            CreateClone(new Vector3(-lengthX, 0, 0));
            CreateClone(new Vector3(lengthX * 2, 0, 0));
            CreateClone(new Vector3(-lengthX * 2, 0, 0));
        }
        if (repeatY && lengthY > 0)
        {
            CreateClone(new Vector3(0, lengthY, 0));
            CreateClone(new Vector3(0, -lengthY, 0));
            CreateClone(new Vector3(0, lengthY * 2, 0));
            CreateClone(new Vector3(0, -lengthY * 2, 0));
        }
    }

    private void FindCamera()
    {
        if (cam == null)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.gameObject;
            }
            else
            {
                Camera fallbackCam = FindFirstObjectByType<Camera>();
                if (fallbackCam != null)
                {
                    cam = fallbackCam.gameObject;
                }
            }
        }
    }


    private void CreateClone(Vector3 offset)
    {
        GameObject clone = new GameObject(gameObject.name + "_clone");
        clone.transform.SetParent(transform);
        clone.transform.position = transform.position + offset;
        clone.transform.localScale = Vector3.one;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();
            cloneSr.sprite = sr.sprite;
            cloneSr.color = sr.color;
            cloneSr.sortingLayerID = sr.sortingLayerID;
            cloneSr.sortingLayerName = sr.sortingLayerName;
            cloneSr.sortingOrder = sr.sortingOrder;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        float tempX = camX * (1 - parallaxEffectX);
        float distX = camX * parallaxEffectX;
        
        float tempY = camY * (1 - parallaxEffectY);
        float distY = camY * parallaxEffectY;

        // Automatically detect if the camera has teleported (e.g. into the menu area or during gameplay transitions)
        // If the camera position is far away from where the background currently is, instantly snap the origin
        float threshX = lengthX > 0 ? lengthX * 1.5f : 50f;
        float threshY = lengthY > 0 ? lengthY * 1.5f : 50f;
        float expectedX = startposX + distX;
        float expectedY = startposY + distY;

        if (Mathf.Abs(camX - expectedX) > threshX || Mathf.Abs(camY - expectedY) > threshY)
        {
            startposX = camX * (1 - parallaxEffectX);
            startposY = camY * (1 - parallaxEffectY);
            Debug.Log($"[ParallaxBackground] Camera teleport detected! Snapped background origin to keep centered: {cam.transform.position}");
        }

        transform.position = new Vector3(startposX + distX, startposY + distY, transform.position.z);

        if (repeatX && lengthX > 0)
        {
            // changed to while loop so it catches up instantly if spawned far away
            while (tempX > startposX + lengthX) startposX += lengthX;
            while (tempX < startposX - lengthX) startposX -= lengthX;
        }
        
        if (repeatY && lengthY > 0)
        {
            while (tempY > startposY + lengthY) startposY += lengthY;
            while (tempY < startposY - lengthY) startposY -= lengthY;
        }
    }
}
