using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float amplitude = 5f;
    public float speed = 2f;
    private RectTransform rectTransform;
    private Vector2 startPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        if (rectTransform != null)
        {
            startPos = rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPos + new Vector2(0, Mathf.Sin(Time.time * speed) * amplitude);
        }
    }

    public void UpdateBasePosition(Vector2 newPos)
    {
        startPos = newPos;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPos + new Vector2(0, Mathf.Sin(Time.time * speed) * amplitude);
        }
    }
}
