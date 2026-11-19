using UnityEngine;

[DisallowMultipleComponent]
public class YSortByPosition : MonoBehaviour
{
    [Tooltip("Geser titik 'kaki' (nilai unit). Biasanya 0 jika pivot objek ada di kaki.")]
    [SerializeField] private float yOffset = 0f;

    [Tooltip("Makin besar makin halus urutan sorting.")]
    [SerializeField] private int multiplier = 100;

    [Tooltip("Matikan untuk objek diam (pohon, bangku) agar lebih ringan.")]
    [SerializeField] private bool updateEveryFrame = true;

    private SpriteRenderer[] renderers;
    private int[] baseOrders;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseOrders = new int[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            baseOrders[i] = renderers[i].sortingOrder;
        }
    }

    private void Start()
    {
        ApplySorting();
    }

    private void LateUpdate()
    {
        if (updateEveryFrame)
        {
            ApplySorting();
        }
    }

    private void ApplySorting()
    {
        int order = -Mathf.RoundToInt((transform.position.y + yOffset) * multiplier);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = order + baseOrders[i];
        }
    }
}
