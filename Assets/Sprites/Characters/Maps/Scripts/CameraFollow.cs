using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Batas Map (unit Unity)")]
    [SerializeField] private Vector2 mapMin = new Vector2(0f, 0f);
    [SerializeField] private Vector2 mapMax = new Vector2(48f, 32f);

    [Header("Anti Getar")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private float pixelsPerUnit = 32f;

    private Camera cam;
    private Vector3 smoothPosition;
    private Vector3 velocity;
    private bool initialized;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        smoothPosition = transform.position;
    }

    private void LateUpdate()
    {
        // Cari Player otomatis kalau Target masih kosong
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        desired = ClampToMap(desired);

        // Frame pertama: langsung loncat ke posisi Player
        if (!initialized)
        {
            smoothPosition = desired;
            initialized = true;
        }

        smoothPosition = Vector3.SmoothDamp(smoothPosition, desired, ref velocity, smoothTime);

        Vector3 finalPos = smoothPosition;
        if (snapToPixelGrid)
        {
            finalPos.x = Mathf.Round(finalPos.x * pixelsPerUnit) / pixelsPerUnit;
            finalPos.y = Mathf.Round(finalPos.y * pixelsPerUnit) / pixelsPerUnit;
        }
        transform.position = finalPos;
    }

    private Vector3 ClampToMap(Vector3 pos)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = mapMin.x + halfW;
        float maxX = mapMax.x - halfW;
        float minY = mapMin.y + halfH;
        float maxY = mapMax.y - halfH;

        pos.x = (minX > maxX) ? (mapMin.x + mapMax.x) * 0.5f : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = (minY > maxY) ? (mapMin.y + mapMax.y) * 0.5f : Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }
}