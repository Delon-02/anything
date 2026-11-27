using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Sorts an object by its foot/base position for a top-down 2D scene.</summary>
[DisallowMultipleComponent]
public class YSort2D : MonoBehaviour
{
    [SerializeField] private Transform sortingPoint;
    [SerializeField] private int sortingOffset;
    [SerializeField] private int precision = 100;

    private SpriteRenderer spriteRenderer;
    private SortingGroup sortingGroup;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sortingGroup = GetComponent<SortingGroup>();
    }

    private void LateUpdate()
    {
        Transform point = sortingPoint != null ? sortingPoint : transform;
        int order = Mathf.Max(0, Mathf.RoundToInt(-point.position.y * precision) + sortingOffset);

        if (sortingGroup != null)
            sortingGroup.sortingOrder = order;
        else if (spriteRenderer != null)
            spriteRenderer.sortingOrder = order;
    }
}
