#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Editor utility: select bakery sprite objects, then run Tools > Bakery > Create Colliders For Selected.</summary>
public static class BakeryColliderSetup
{
    private struct Preset
    {
        public float width;
        public float height;
        public bool noCollision;
        public bool pillars;
        public Preset(float width, float height) { this.width = width; this.height = height; noCollision = false; pillars = false; }
    }

    [MenuItem("Tools/Bakery/Create Colliders For Selected")]
    private static void CreateForSelected()
    {
        int added = 0, skipped = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) { skipped++; continue; }

            Preset p = GetPreset(go.name);
            if (p.noCollision) { skipped++; continue; }

            Undo.RegisterFullObjectHierarchyUndo(go, "Create Bakery Colliders");
            Bounds b = sr.sprite.bounds;

            if (p.pillars)
            {
                added += CreatePillarColliders(go, b);
                continue;
            }

            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            if (col == null) col = Undo.AddComponent<BoxCollider2D>(go);
            float h = Mathf.Max(0.01f, b.size.y * p.height);
            col.size = new Vector2(Mathf.Max(0.01f, b.size.x * p.width), h);
            col.offset = new Vector2(b.center.x, b.min.y + h * 0.5f);
            added++;
        }

        Debug.Log($"Bakery collision setup selesai. Collider dibuat/diperbarui: {added}; dilewati: {skipped}.");
    }

    [MenuItem("Tools/Bakery/Create Colliders For Selected", true)]
    private static bool ValidateCreate() => Selection.gameObjects.Length > 0;

    [MenuItem("Tools/Bakery/Remove Generated Pillar Colliders From Selected")]
    private static void RemovePillars()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            foreach (Transform child in go.transform)
            {
                if (child.name.StartsWith("BakeryCollision_")) Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }

    private static int CreatePillarColliders(GameObject go, Bounds b)
    {
        // This sprite contains two entrance pillars and a walkable mat between them.
        float pillarWidth = b.size.x * 0.20f;
        float pillarHeight = b.size.y * 0.24f;
        float y = b.min.y + pillarHeight * 0.5f;
        CreateChildBox(go, "BakeryCollision_LeftPillar", new Vector2(b.min.x + pillarWidth * 0.5f, y), new Vector2(pillarWidth, pillarHeight));
        CreateChildBox(go, "BakeryCollision_RightPillar", new Vector2(b.max.x - pillarWidth * 0.5f, y), new Vector2(pillarWidth, pillarHeight));
        return 2;
    }

    private static void CreateChildBox(GameObject parent, string childName, Vector2 offset, Vector2 size)
    {
        Transform existing = parent.transform.Find(childName);
        GameObject child;
        if (existing != null) child = existing.gameObject;
        else
        {
            child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, "Create Bakery Pillar Collider");
            child.transform.SetParent(parent.transform, false);
        }
        child.transform.localPosition = offset;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        BoxCollider2D box = child.GetComponent<BoxCollider2D>();
        if (box == null) box = Undo.AddComponent<BoxCollider2D>(child);
        box.offset = Vector2.zero;
        box.size = size;
    }

    private static Preset GetPreset(string objectName)
    {
        string n = objectName.ToLowerInvariant();
        // Back/wall-mounted decorations should not block walking.
        if (n.Contains("bakery_sign") || n.Contains("ceiling_lamps")) return new Preset(0f, 0f) { noCollision = true };
        if (n.Contains("entrance_mat_pillars")) return new Preset(0f, 0f) { pillars = true };

        // Partial colliders sit along the lower/front edge so the player can walk behind tall props.
        if (n.Contains("cashier_counter")) return new Preset(0.94f, 0.24f);
        if (n.Contains("middle_shelf")) return new Preset(0.90f, 0.22f);
        if (n.Contains("bottom_left_shelf")) return new Preset(0.92f, 0.25f);
        if (n.Contains("bottom_right_equipment_table")) return new Preset(0.92f, 0.24f);
        if (n.Contains("left_ingredient_display")) return new Preset(0.88f, 0.22f);
        if (n.Contains("right_sack_display")) return new Preset(0.88f, 0.20f);
        if (n.Contains("wall_left_shelf") || n.Contains("wall_center_shelf") || n.Contains("wall_right_shelf")) return new Preset(0.90f, 0.20f);
        if (n.Contains("mixer_utensils_station")) return new Preset(0.88f, 0.24f);
        if (n.Contains("right_tray_rack")) return new Preset(0.86f, 0.20f);
        if (n.Contains("left_blackboard")) return new Preset(0.75f, 0.18f);

        // Unknown selected sprites get a conservative bottom strip, not a full-image collider.
        return new Preset(0.85f, 0.20f);
    }
}
#endif
