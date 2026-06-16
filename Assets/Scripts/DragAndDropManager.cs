using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Canvas parentCanvas;

    public Action onDroppedCorrectly;
    public bool isDropped = false;

    [Header("Game Feel")]
    public float dragScale = 1.15f;
    public float scaleSpeed = 15f;
    public AudioClip pickupSound;
    public AudioClip dropSound;
    private Coroutine scaleRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (bootstrap != null)
        {
            if (pickupSound == null) pickupSound = bootstrap.buttonHoverSound;
            if (dropSound == null) dropSound = bootstrap.buttonClickSound;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropped) return;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;

        if (AudioManager.Instance != null && pickupSound != null)
        {
            AudioManager.Instance.PlaySFX(pickupSound);
        }

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(dragScale));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDropped) return;
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDropped) return;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!isDropped)
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(1f));
    }

    public void PlayDropSound()
    {
        if (AudioManager.Instance != null && dropSound != null)
        {
            AudioManager.Instance.PlaySFX(dropSound);
        }
    }

    private IEnumerator ScaleTo(float targetScale)
    {
        Vector3 target = new Vector3(targetScale, targetScale, 1f);
        while (Vector3.Distance(transform.localScale, target) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * scaleSpeed);
            yield return null;
        }
        transform.localScale = target;
    }
}

public class DropZone : MonoBehaviour, IDropHandler
{
    public Action onDropSuccess;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragItem dragItem = eventData.pointerDrag.GetComponent<DragItem>();
            if (dragItem != null && !dragItem.isDropped)
            {
                dragItem.isDropped = true;
                dragItem.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
                dragItem.PlayDropSound();
                onDropSuccess?.Invoke();
            }
        }
    }
}
