using UnityEngine;
using System.Collections.Generic;

public class CharacterMovementWithAnimator : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    private Animator childAnimator;
    private Vector3 inputDirection;
    private float locomotionSpeed;
    private float currentSpeed;

    private Dictionary<KeyCode, float> lastTapTimes = new Dictionary<KeyCode, float>();
    private float doubleTapThreshold = 0.3f;
    private bool isRunning = false;

    private readonly KeyCode[] movementKeys = new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    void Start()
    {
        childAnimator = GetComponentInChildren<Animator>();
        if (childAnimator == null)
        {
            Debug.LogWarning("No Animator found in child of " + gameObject.name);
        }

        foreach (KeyCode key in movementKeys)
        {
            lastTapTimes[key] = -1f;
        }
    }

    void Update()
    {
        Vector3 newDirection = Vector3.zero;

        // Handle input and double-tap detection
        foreach (KeyCode key in movementKeys)
        {
            if (Input.GetKeyDown(key))
            {
                float time = Time.time;

                if (time - lastTapTimes[key] <= doubleTapThreshold)
                {
                    // Double-tap detected -> start running
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

        // Update movement speed based on state
        if (inputDirection != Vector3.zero)
        {
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }
        else
        {
            currentSpeed = 0f;
            isRunning = false; // Stop running when no keys are held
        }

        // Move character
        if (inputDirection != Vector3.zero)
        {
            transform.Translate(inputDirection * currentSpeed * Time.deltaTime, Space.World);

            Quaternion toRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }

        // Set animator value
        locomotionSpeed = currentSpeed == 0f ? 0f : (isRunning ? 1f : 0.5f);
        if (childAnimator != null)
        {
            childAnimator.SetFloat("LocomotionSpeed", locomotionSpeed);
        }
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
