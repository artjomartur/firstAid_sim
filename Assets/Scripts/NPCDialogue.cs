using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NPCDialogue : MonoBehaviour
{
    private GameObject bubbleObj;
    private Text dialogueText;
    private NPCWander npcWander;

    private string[] panicTexts = {
        "Oh mein Gott!",
        "Bitte hilf ihm!",
        "Ich habe schon 112 gerufen!",
        "Ist er tot?!",
        "Wir brauchen einen Arzt!",
        "Was ist passiert?",
        "Nicht bewegen!"
    };

    private string[] chatterTexts = {
        "Schönes Wetter heute!",
        "Hier ist es so friedlich.",
        "Ich liebe diesen Park.",
        "Hoffentlich passiert nichts...",
        "Was für ein schöner Tag!"
    };

    void Start()
    {
        npcWander = GetComponent<NPCWander>();
        CreateDialogueBubble();
        StartCoroutine(RandomDialogueRoutine());
    }

    private void CreateDialogueBubble()
    {
        bubbleObj = new GameObject("DialogueBubble");
        bubbleObj.transform.SetParent(transform);
        float yOffset = Random.Range(2.0f, 3.2f); // Random Y to prevent overlap
        bubbleObj.transform.localPosition = new Vector3(0, yOffset, 0);

        Canvas canvas = bubbleObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform rt = bubbleObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 100);
        rt.localScale = new Vector3(0.02f, 0.02f, 1f); 
        
        canvas.sortingLayerName = "Layer 3";
        canvas.sortingOrder = 200;

        Image bg = bubbleObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.75f); // Darker for better contrast

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubbleObj.transform);
        dialogueText = textObj.AddComponent<Text>();
        dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAnchor.MiddleCenter;
        dialogueText.fontSize = 24;
        dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        
        RectTransform trt = textObj.GetComponent<RectTransform>();
        trt.sizeDelta = new Vector2(280, 80);
        trt.localPosition = Vector3.zero;
        trt.localScale = Vector3.one;

        bubbleObj.SetActive(false);
    }

    private IEnumerator RandomDialogueRoutine()
    {
        while (true)
        {
            // Wait interval
            yield return new WaitForSeconds(Random.Range(5f, 15f));
            
            bool isPanic = (npcWander != null && npcWander.isPanicking);
            
            // Choose text pool
            string[] activePool = isPanic ? panicTexts : chatterTexts;
            dialogueText.text = activePool[Random.Range(0, activePool.Length)];
            
            // Color coding
            dialogueText.color = isPanic ? new Color(1f, 0.4f, 0.4f) : Color.white;
            
            bubbleObj.SetActive(true);
            
            yield return new WaitForSeconds(3f);
            
            bubbleObj.SetActive(false);
        }
    }

    void LateUpdate()
    {
        // Prevent bubble from flipping when parent flips its scale
        if (bubbleObj != null)
        {
            // Reset rotation to avoid compounding transformations
            bubbleObj.transform.localEulerAngles = Vector3.zero;
            
            // Check world scale and flip local scale if necessary to keep world scale positive
            Vector3 worldScale = bubbleObj.transform.lossyScale;
            if (worldScale.x < 0)
            {
                Vector3 localScale = bubbleObj.transform.localScale;
                localScale.x = -localScale.x;
                bubbleObj.transform.localScale = localScale;
            }
        }
    }
}
