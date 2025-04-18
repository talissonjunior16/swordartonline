using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CharacterMovementWithAnimator : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;

    private Animator childAnimator;
    private Vector3 inputDirection;
    private float currentSpeed;
    private float locomotionSpeed;

    private Dictionary<KeyCode, float> lastTapTimes = new Dictionary<KeyCode, float>();
    private float doubleTapThreshold = 0.3f;
    private bool isRunning = false;

    private readonly KeyCode[] movementKeys = new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    void Start()
    {
        InitializeAnimator();
        InitializeTapTimes();
    }

    void Update()
    {
        HandleMovementInput();
        UpdateMovement();
        UpdateAnimator();
        HandleAttack();
        HandleDash();
    }

    private void InitializeAnimator()
    {
        childAnimator = GetComponentInChildren<Animator>();
        if (childAnimator == null)
        {
            Debug.LogWarning("No Animator found in child of " + gameObject.name);
        }
    }

    private void InitializeTapTimes()
    {
        foreach (KeyCode key in movementKeys)
        {
            lastTapTimes[key] = -1f;
        }
    }

    private void HandleMovementInput()
    {
        Vector3 newDirection = Vector3.zero;

        foreach (KeyCode key in movementKeys)
        {
            if (Input.GetKeyDown(key))
            {
                float time = Time.time;

                if (time - lastTapTimes[key] <= doubleTapThreshold)
                {
                    isRunning = true;
                }

                lastTapTimes[key] = time;
            }

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

            transform.Translate(inputDirection * currentSpeed * Time.deltaTime, Space.World);

            Quaternion toRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
        else
        {
            currentSpeed = 0f;
            isRunning = false;
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
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private Vector3 KeyToDirection(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return Vector3.forward;
            case KeyCode.S: return Vector3.back;
            case KeyCode.A: return Vector3.left;
            case KeyCode.D: return Vector3.right;
            default: return Vector3.zero;
        }
    }
}
