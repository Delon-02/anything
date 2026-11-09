using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;

    private bool isRunning;
    private string currentAnimation = "";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Mengambil Animator dari Player.
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError(
                "Animator tidak ditemukan pada Player!",
                this
            );
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        // Input WASD atau tombol panah.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveInput = Vector2.ClampMagnitude(
            new Vector2(horizontal, vertical),
            1f
        );

        // Shift untuk berlari.
        isRunning =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        // Menyimpan arah terakhir.
        if (moveInput.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                lastDirection = new Vector2(
                    Mathf.Sign(moveInput.x), 0f
                );
            }
            else
            {
                lastDirection = new Vector2(
                    0f, Mathf.Sign(moveInput.y)
                );
            }
        }
    }

    private void MovePlayer()
    {
        if (rb == null) return;

        float currentSpeed = isRunning
            ? runSpeed
            : walkSpeed;

        Vector2 movement = moveInput * currentSpeed;

        rb.MovePosition(
            rb.position +
            movement * Time.fixedDeltaTime
        );
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        string direction;

        // Menentukan arah karakter.
        if (lastDirection.y > 0f)
        {
            direction = "up";
        }
        else if (lastDirection.y < 0f)
        {
            direction = "down";
        }
        else if (lastDirection.x < 0f)
        {
            direction = "left";
        }
        else
        {
            direction = "right";
        }

        string animationName;

        // Menentukan jenis animasi.
        if (moveInput.sqrMagnitude < 0.01f)
        {
            animationName = "idle " + direction;
        }
        else if (isRunning)
        {
            animationName = "run " + direction;
        }
        else
        {
            animationName = "walk " + direction;
        }

        // Mengganti animasi hanya jika state berubah.
        if (currentAnimation != animationName)
        {
            currentAnimation = animationName;

            animator.Play(
                "Base Layer." + animationName,
                0,
                0f
            );
        }
    }
}