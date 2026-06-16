using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AEDManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject aedPanel;
    public RectTransform pad1, pad2; // The pads to drag
    public RectTransform target1, target2; // The targets on the body
    public Button shockButton;
    public Text instructionText;
    
    private bool pad1Placed = false;
    private bool pad2Placed = false;
    private bool isCharging = false;

    public void Activate()
    {
        aedPanel.SetActive(true);
        pad1Placed = false;
        pad2Placed = false;
        isCharging = false;
        shockButton.interactable = false;
        instructionText.text = "PLATZIERE DIE ELEKTRODEN!";
        
        // Reset positions
        pad1.anchoredPosition = new Vector2(-450, 0);
        pad2.anchoredPosition = new Vector2(-450, -200);
        
        // Ensure DragItem is present and RESET
        DragItem d1 = pad1.gameObject.GetComponent<DragItem>();
        if (d1 == null) d1 = pad1.gameObject.AddComponent<DragItem>();
        d1.isDropped = false;
        
        DragItem d2 = pad2.gameObject.GetComponent<DragItem>();
        if (d2 == null) d2 = pad2.gameObject.AddComponent<DragItem>();
        d2.isDropped = false;
    }

    private void Update()
    {
        if (!aedPanel.activeSelf || isCharging) return;

        float snapDist = 80f; // Increased for better feel

        if (!pad1Placed && Vector2.Distance(pad1.anchoredPosition, target1.anchoredPosition) < snapDist)
        {
            pad1Placed = true;
            pad1.anchoredPosition = target1.anchoredPosition;
            target1.GetComponent<Image>().color = new Color(1,1,1,0.5f); // Semi-transparent white overlay
            
            DragItem di = pad1.GetComponent<DragItem>();
            if (di != null) di.isDropped = true; // Prevent snap-back
            
            ScoreManager.Instance?.RecordSuccess();
            CheckPads();
        }

        if (!pad2Placed && Vector2.Distance(pad2.anchoredPosition, target2.anchoredPosition) < snapDist)
        {
            pad2Placed = true;
            pad2.anchoredPosition = target2.anchoredPosition;
            target2.GetComponent<Image>().color = new Color(1,1,1,0.5f); // Semi-transparent white overlay
            
            DragItem di = pad2.GetComponent<DragItem>();
            if (di != null) di.isDropped = true; // Prevent snap-back
            
            ScoreManager.Instance?.RecordSuccess();
            CheckPads();
        }
    }

    private void CheckPads()
    {
        if (pad1Placed && pad2Placed)
        {
            StartCoroutine(ChargeAED());
        }
    }

    private IEnumerator ChargeAED()
    {
        isCharging = true;
        instructionText.text = "ANALYSE... NICHT BERÜHREN!";
        yield return new WaitForSeconds(3f);
        
        instructionText.text = "SCHOCK EMPFOHLEN! JETZT DRÜCKEN!";
        shockButton.interactable = true;
    }

    public void OnShockPressed()
    {
        shockButton.interactable = false;
        instructionText.text = "SCHOCK ABGEGEBEN!";
        ScoreManager.Instance?.RecordSuccess();
        StartCoroutine(FinishAED());
    }

    private IEnumerator FinishAED()
    {
        yield return new WaitForSeconds(2f);
        aedPanel.SetActive(false);
        gameManager.OnAEDCompleted();
    }
}
