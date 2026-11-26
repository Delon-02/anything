using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Horizontal = A/D dan panah kiri/kanan, Vertical = W/S dan panah atas/bawah
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        // Gerak diagonal tidak boleh lebih cepat
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = input * moveSpeed;
    }
}
