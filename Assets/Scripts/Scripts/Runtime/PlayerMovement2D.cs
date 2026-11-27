using UnityEngine;

/// <summary>Top-down movement for Unity 2022 LTS using Rigidbody2D physics.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = Vector2.ClampMagnitude(input, 1f);
    }

    private void FixedUpdate()
    {
        // Unity 2022 LTS uses Rigidbody2D.velocity (not linearVelocity).
        rb.velocity = input * moveSpeed;
    }
}
