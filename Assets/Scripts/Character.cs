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
    private float locomotionSpeed;
    private Rigidbody rb;

    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
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

        UpdateAnimator(isRunning ? runSpeed : walkSpeed, inputDirection);

        if (Input.GetMouseButtonDown(0) && childAnimator != null)
            childAnimator.SetTrigger("Attack");

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && inputDirection != Vector3.zero)
        {
            if (childAnimator != null)
                childAnimator.SetTrigger("Dash");

            RequestDashServerRpc(inputDirection);
        }
    }

    [ServerRpc]
    private void SendMovementInputServerRpc(Vector3 direction, bool isRunning)
    {
        if (direction == Vector3.zero || isDashing) return;

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.fixedDeltaTime));
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
            Debug.LogWarning("No Animator found in child of " + gameObject.name);
    }
}
