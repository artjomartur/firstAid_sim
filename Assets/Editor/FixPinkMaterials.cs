using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FixPinkMaterials : EditorWindow
{
    [MenuItem("Tools/Fix Pink Materials (2D)")]
    public static void FixMaterials()
    {
        int count = 0;
        
        // Find the default sprite material
        Material defaultSpriteMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        if (defaultSpriteMaterial == null)
        {
            Debug.LogError("Could not find Sprites-Default material!");
            return;
        }

        // Fix all SpriteRenderers
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>(true);
        foreach (var sr in spriteRenderers)
        {
            if (sr.sharedMaterial == null || sr.sharedMaterial.name.Contains("Pink") || sr.sharedMaterial.shader.name.Contains("Error") || sr.sharedMaterial.shader.name.Contains("Lit"))
            {
                sr.sharedMaterial = defaultSpriteMaterial;
                EditorUtility.SetDirty(sr);
                count++;
            }
        }

        // Fix all TilemapRenderers
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>(true);
        foreach (var tr in tilemapRenderers)
        {
            if (tr.sharedMaterial == null || tr.sharedMaterial.name.Contains("Pink") || tr.sharedMaterial.shader.name.Contains("Error") || tr.sharedMaterial.shader.name.Contains("Lit") || tr.sharedMaterial.name != "Sprites-Default")
            {
                tr.sharedMaterial = defaultSpriteMaterial;
                EditorUtility.SetDirty(tr);
                count++;
            }
        }

        Debug.Log($"Fixed {count} pink materials. Swapped to Sprites-Default.");
    }
}
