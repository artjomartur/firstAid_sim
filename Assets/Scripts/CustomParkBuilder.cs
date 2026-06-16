using UnityEngine;
using System.Collections.Generic;

public class CustomParkBuilder : MonoBehaviour
{
    public static CustomParkBuilder Instance { get; private set; }
    
    private GameObject parkContainer;
    private const int GridSizeX = 12;
    private const int GridSizeY = 8;
    
    // Define the world boundaries for the 12x8 grid
    private Vector2 gridOrigin = new Vector2(-15, 10);
    private float cellSize = 2.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        BuildPark();
    }

    public void BuildPark()
    {
        if (parkContainer != null) Destroy(parkContainer);
        parkContainer = new GameObject("CustomParkDecorations");
        
        string data = PlayerPrefs.GetString("CustomParkData", "");
        if (string.IsNullOrEmpty(data)) return;
        
        string[] cells = data.Split(',');
        if (cells.Length == GridSizeX * GridSizeY)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    int val;
                    if (int.TryParse(cells[y * GridSizeX + x], out val))
                    {
                        if (val > 0)
                        {
                            Vector3 pos = new Vector3(gridOrigin.x + (x * cellSize), gridOrigin.y - (y * cellSize), 0);
                            SpawnDecoration(val, pos);
                        }
                    }
                }
            }
        }
    }
    
    private void SpawnDecoration(int type, Vector3 pos)
    {
        GameObject dec = null;
        if (type == 1) // Tree
        {
            dec = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dec.name = "CustomTree";
            dec.transform.localScale = new Vector3(1, 2, 1);
            dec.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.2f);
        }
        else if (type == 2) // Bench
        {
            dec = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dec.name = "CustomBench";
            dec.transform.localScale = new Vector3(2, 0.5f, 0.8f);
            dec.GetComponent<Renderer>().material.color = new Color(0.6f, 0.3f, 0.1f);
        }
        else if (type == 3) // Bush
        {
            dec = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dec.name = "CustomBush";
            dec.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            dec.GetComponent<Renderer>().material.color = new Color(0.1f, 0.5f, 0.1f);
        }
        
        if (dec != null)
        {
            dec.transform.position = pos;
            dec.transform.SetParent(parkContainer.transform);
            
            // Remove collider if you don't want it to block the player (optional)
            // Destroy(dec.GetComponent<Collider>());
        }
    }
}
