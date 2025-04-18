using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody))]
public class Character : NetworkBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;

    private Animator childAnimator;
    private Vector3 inputDirection;
    private float currentSpeed;
    private float locomotionSpeed;

    private Rigidbody rb;

    private readonly KeyCode[] movementKeys = new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;

        if (!IsOwner) return;

        InitializeAnimator();
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMovementInput();
        UpdateAnimator();
        HandleAttack();
        HandleDash();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        UpdateMovement();
    }

    private void InitializeAnimator()
    {
        childAnimator = GetComponentInChildren<Animator>();
        if (childAnimator == null)
        {
            Debug.LogWarning("No Animator found in child of " + gameObject.name);
        }
    }

    private void HandleMovementInput()
    {
        Vector3 newDirection = Vector3.zero;

        foreach (KeyCode key in movementKeys)
        {
            if (Input.GetKey(key))
            {
                newDirection += KeyToDirection(key);
            }
        }

        inputDirection = newDirection.normalized;
    }

    private void UpdateMovement()
    {
        if (inputDirection != Vector3.zero)
        {
            currentSpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? runSpeed : walkSpeed;

            Vector3 newPosition = rb.position + inputDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            Quaternion toRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.fixedDeltaTime));
        }
        else
        {
            currentSpeed = 0f;
        }
    }

    private void UpdateAnimator()
    {
        locomotionSpeed = currentSpeed == 0f ? 0f : (currentSpeed == runSpeed ? 1f : 0.5f);
        if (childAnimator != null)
        {
            childAnimator.SetFloat("LocomotionSpeed", locomotionSpeed);
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && childAnimator != null)
        {
            childAnimator.SetTrigger("Attack");
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (childAnimator != null)
            {
                childAnimator.SetTrigger("Dash");
            }

            if (inputDirection != Vector3.zero)
            {
                Vector3 dashStartPos = transform.position;
                Vector3 dashTargetPos = transform.position + inputDirection * dashDistance;
                StartCoroutine(SmoothDash(dashStartPos, dashTargetPos, Time.time));
            }
        }
    }

    private IEnumerator SmoothDash(Vector3 startPosition, Vector3 targetPosition, float startTime)
    {
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / dashDuration;

            if (rb)
                rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, t));

            yield return null;
        }

        if (rb)
            rb.MovePosition(targetPosition);
    }

    private Vector3 KeyToDirection(KeyCode key)
    {
        return key switch
        {
            KeyCode.W => Vector3.forward,
            KeyCode.S => Vector3.back,
            KeyCode.A => Vector3.left,
            KeyCode.D => Vector3.right,
            _ => Vector3.zero,
        };
    }
}
