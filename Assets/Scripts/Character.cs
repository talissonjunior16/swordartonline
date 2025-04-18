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
    private float currentSpeed;
    private float locomotionSpeed;
    private Rigidbody rb;

    private Vector3 cachedInputDirection = Vector3.zero;
    private bool isDashing = false;

    private readonly KeyCode[] movementKeys = new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            InitializeAnimator();
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        Vector3 inputDirection = Vector3.zero;

        foreach (KeyCode key in movementKeys)
        {
            if (Input.GetKey(key))
            {
                inputDirection += KeyToDirection(key);
            }
        }

        inputDirection = inputDirection.normalized;
        bool run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        SendMovementServerRpc(inputDirection, run);

        UpdateAnimator(run ? runSpeed : walkSpeed, inputDirection);

        if (Input.GetMouseButtonDown(0) && childAnimator != null)
        {
            childAnimator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && inputDirection != Vector3.zero)
        {
            if (childAnimator != null)
                childAnimator.SetTrigger("Dash");

            DashServerRpc(inputDirection);
        }
    }

    [ServerRpc]
    private void SendMovementServerRpc(Vector3 direction, bool run)
    {
        if (direction == Vector3.zero) return;

        currentSpeed = run ? runSpeed : walkSpeed;
        Vector3 newPosition = rb.position + direction * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.fixedDeltaTime));
    }

    [ServerRpc]
    private void DashServerRpc(Vector3 direction)
    {
        if (!isDashing)
        {
            StartCoroutine(SmoothDash(direction));
        }
    }

    private IEnumerator SmoothDash(Vector3 direction)
    {
        isDashing = true;
        Vector3 dashStartPos = transform.position;
        Vector3 dashTargetPos = transform.position + direction * dashDistance;
        float startTime = Time.time;

        while (Time.time - startTime < dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            rb.MovePosition(Vector3.Lerp(dashStartPos, dashTargetPos, t));
            yield return null;
        }

        rb.MovePosition(dashTargetPos);
        isDashing = false;
    }

    private void UpdateAnimator(float speed, Vector3 direction)
    {
        locomotionSpeed = direction == Vector3.zero ? 0f : (speed == runSpeed ? 1f : 0.5f);
        if (childAnimator != null)
        {
            childAnimator.SetFloat("LocomotionSpeed", locomotionSpeed);
        }
    }

    private void InitializeAnimator()
    {
        childAnimator = GetComponentInChildren<Animator>();
        if (childAnimator == null)
        {
            Debug.LogWarning("No Animator found in child of " + gameObject.name);
        }
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