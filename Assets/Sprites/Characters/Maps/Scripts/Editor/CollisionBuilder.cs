#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CollisionBuilder
{
    [MenuItem("Tools/French Town/Build Collision From CSV")]
    public static void Build()
    {
        string[] csvGuids = AssetDatabase.FindAssets("collision_grid_48x32 t:TextAsset");
        if (csvGuids.Length == 0)
        {
            Debug.LogError("File collision_grid_48x32.csv tidak ditemukan di Assets.");
            return;
        }
        TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(csvGuids[0]));

        string[] tileGuids = AssetDatabase.FindAssets("SolidTile t:TileBase");
        if (tileGuids.Length == 0)
        {
            Debug.LogError("Tile bernama SolidTile tidak ditemukan. Buat dulu lewat Create > 2D > Tiles > Tile.");
            return;
        }
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(tileGuids[0]));

        GameObject go = GameObject.Find("Collision_Tilemap");
        if (go == null)
        {
            Debug.LogError("GameObject Collision_Tilemap tidak ditemukan di scene.");
            return;
        }
        Tilemap tilemap = go.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Collision_Tilemap tidak punya komponen Tilemap.");
            return;
        }

        tilemap.ClearAllTiles();

        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        int rows = lines.Length;
        int count = 0;

        for (int r = 0; r < rows; r++)
        {
            string[] cells = lines[r].Split(',');
            for (int c = 0; c < cells.Length; c++)
            {
                if (cells[c].Trim() == "1")
                {
                    // Baris 0 di CSV = paling atas, di Unity y paling atas = rows - 1
                    tilemap.SetTile(new Vector3Int(c, rows - 1 - r, 0), tile);
                    count++;
                }
            }
        }

        EditorUtility.SetDirty(tilemap);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("Collision selesai dibuat: " + count + " tile solid.");
    }
}
#endif
