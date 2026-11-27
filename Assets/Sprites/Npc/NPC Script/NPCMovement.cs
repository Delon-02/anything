using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator NPC tidak ditemukan!");
            return;
        }

        // Memainkan animasi satu kali saat mulai
        animator.Play("walk right", 0, 0f);

        Debug.Log("NPC berhasil dijalankan!");
    }

    void Update()
    {
        // NPC berjalan ke kanan
        transform.Translate(
            Vector3.right * speed * Time.deltaTime
        );
    }
}