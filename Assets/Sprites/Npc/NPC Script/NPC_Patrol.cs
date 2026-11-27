using UnityEngine;
using System.Collections;

public class CustomerNPC : MonoBehaviour
{
    public enum CustomerState
    {
        Entering,
        WalkingToQueue,
        Waiting,
        WalkingToSeat,
        Eating,
        Leaving
    }

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.1f;

    [Header("Target Positions")]
    public Transform entrancePoint;
    public Transform queuePoint;
    public Transform seatPoint;
    public Transform exitPoint;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Activity Time")]
    public float waitingTime = 3f;
    public float eatingTime = 8f;

    public CustomerState currentState;

    private Transform target;
    private bool isMoving;

    void Start()
    {
        currentState = CustomerState.Entering;

        target = entrancePoint;

        StartCoroutine(CustomerRoutine());
    }

    void Update()
    {
        MoveCustomer();
    }

    void MoveCustomer()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            target.position
        );

        if (distance > stopDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            isMoving = true;

            UpdateAnimation();
        }
        else
        {
            isMoving = false;
            UpdateAnimation();
        }
    }

    IEnumerator CustomerRoutine()
    {
        // Masuk ke toko
        currentState = CustomerState.Entering;
        target = entrancePoint;

        yield return WaitUntilArrived();

        // Menuju antrean
        currentState = CustomerState.WalkingToQueue;
        target = queuePoint;

        yield return WaitUntilArrived();

        // Menunggu pelayanan
        currentState = CustomerState.Waiting;
        yield return new WaitForSeconds(waitingTime);

        // Menuju tempat duduk
        currentState = CustomerState.WalkingToSeat;
        target = seatPoint;

        yield return WaitUntilArrived();

        // Menikmati makanan
        currentState = CustomerState.Eating;
        yield return new WaitForSeconds(eatingTime);

        // Keluar toko
        currentState = CustomerState.Leaving;
        target = exitPoint;

        yield return WaitUntilArrived();

        Destroy(gameObject);
    }

    IEnumerator WaitUntilArrived()
    {
        while (target != null &&
            Vector2.Distance(
                transform.position,
                target.position
            ) > stopDistance)
        {
            yield return null;
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", isMoving);
        }

        if (spriteRenderer != null && isMoving)
        {
            float direction =
                target.position.x - transform.position.x;

            if (Mathf.Abs(direction) > 0.01f)
            {
                spriteRenderer.flipX = direction < 0;
            }
        }
    }
}