using UnityEngine;

public class EnvironmentAnimator : MonoBehaviour
{
    void Start()
    {
        // Find all objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("tree") || obj.name.ToLower().Contains("bush"))
            {
                // Only add if it has a SpriteRenderer (don't add to empty parents)
                if (obj.GetComponent<SpriteRenderer>() != null && obj.GetComponent<WindSway>() == null)
                {
                    obj.AddComponent<WindSway>();
                }
            }
        }
    }
}

public class WindSway : MonoBehaviour
{
    public float swayAmount = 2f; // degrees of rotation
    public float swaySpeed = 1f;

    private float randomOffset;
    private Quaternion startRot;

    void Start()
    {
        randomOffset = Random.Range(0f, 10f); // Make trees sway out of sync
        startRot = transform.rotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAmount;
        transform.rotation = startRot * Quaternion.Euler(0, 0, angle);
    }
}
