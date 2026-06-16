using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public bool IsTutorialCompleted => PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

    private GameObject tutorialArrow;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (!IsTutorialCompleted)
        {
            StartTutorial();
        }
    }

    private void StartTutorial()
    {
        // Find the first door that leads indoors
        DoorTransition[] doors = FindObjectsOfType<DoorTransition>();
        DoorTransition exteriorDoor = null;
        
        foreach (var door in doors)
        {
            if (door.targetIsIndoors)
            {
                exteriorDoor = door;
                break;
            }
        }

        if (exteriorDoor != null)
        {
            // Create a floating arrow pointing to the door
            tutorialArrow = new GameObject("TutorialArrow");
            tutorialArrow.transform.position = exteriorDoor.transform.position + new Vector3(0, 2f, 0);
            
            SpriteRenderer sr = tutorialArrow.AddComponent<SpriteRenderer>();
            
            // Try to find the arrow sprite from GameBootstrap
            GameBootstrap bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap != null && bootstrap.arrowSprite != null)
            {
                sr.sprite = bootstrap.arrowSprite;
            }

            sr.color = Color.yellow;
            sr.sortingOrder = 999;

            // Make it float
            FloatingUI floatUI = tutorialArrow.AddComponent<FloatingUI>();
            floatUI.amplitude = 0.5f;
            floatUI.speed = 3f;

            Debug.Log("Tutorial gestartet: Bitte das Haus betreten!");
        }
    }

    public void CompleteTutorial()
    {
        if (!IsTutorialCompleted)
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            if (tutorialArrow != null)
            {
                Destroy(tutorialArrow);
            }

            Debug.Log("Tutorial abgeschlossen!");
        }
    }

    // Call this if we want to test the tutorial again
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("Tutorial resettet. Bitte Spiel neu starten.");
    }
}
