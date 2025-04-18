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
    private Rigidbody rb;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InitializeAnimator();
    }

    void Update()
    {
        if (!IsOwner) return;

        Vector3 inputDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) inputDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDirection += Vector3.right;

        inputDirection.Normalize();
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        SendMovementInputServerRpc(inputDirection, isRunning);

        if (Input.GetMouseButtonDown(0))
            RequestAttackAnimationServerRpc();

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && inputDirection != Vector3.zero)
        {
            RequestDashServerRpc(inputDirection);
            RequestDashAnimationServerRpc();
        }
    }

    [ServerRpc]
    private void SendMovementInputServerRpc(Vector3 direction, bool isRunning)
    {
        float locomotionSpeed = 0f;

        if (!isDashing)
        {
            if (direction != Vector3.zero)
            {
                float speed = isRunning ? runSpeed : walkSpeed;
                locomotionSpeed = isRunning ? 1f : 0.5f;

                Vector3 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

                Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.fixedDeltaTime));
            }
            else
            {
                locomotionSpeed = 0f;
            }

            if (childAnimator != null)
                childAnimator.SetFloat("LocomotionSpeed", locomotionSpeed); // synced by NetworkAnimator
        }
    }

    [ServerRpc]
    private void RequestAttackAnimationServerRpc()
    {
        if (childAnimator != null)
            childAnimator.SetTrigger("Attack"); // synced by NetworkAnimator
    }

    [ServerRpc]
    private void RequestDashAnimationServerRpc()
    {
        if (childAnimator != null)
            childAnimator.SetTrigger("Dash"); // synced by NetworkAnimator
    }

    [ServerRpc]
    private void RequestDashServerRpc(Vector3 direction)
    {
        if (!isDashing && direction != Vector3.zero)
        {
            StartCoroutine(SmoothDash(direction));
        }
    }

    private IEnumerator SmoothDash(Vector3 direction)
    {
        isDashing = true;

        Vector3 start = transform.position;
        Vector3 end = start + direction * dashDistance;
        float startTime = Time.time;

        while (Time.time - startTime < dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            rb.MovePosition(Vector3.Lerp(start, end, t));
            yield return null;
        }

        rb.MovePosition(end);
        isDashing = false;
    }

    private void InitializeAnimator()
    {
        childAnimator = GetComponentInChildren<Animator>();
        if (childAnimator == null)
            Debug.LogWarning("No Animator found in child of " + gameObject.name);
    }
}