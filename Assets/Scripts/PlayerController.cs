using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkingSpeed;    // Speed when walking
    [SerializeField] float runningSpeed;    // Speed when running

    [SerializeField] float idleRotationSpeed;     
    [SerializeField] float walkingRotationSpeed;
    [SerializeField] float runningRotationSpeed;

    private Rigidbody body;
    private Animator animator;

    private float currentSpeed;
    private Vector3 moveDir;
    private Vector3 rotation;


    // Start is called before the first frame update
    void Start()
    {
        // Get rigid body component
        body = GetComponent<Rigidbody>();
        // Get animator component
        animator = GetComponent<Animator>();
        // Set initial velocity to zero
        currentSpeed = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Set move direction and rotation to zero
        moveDir = Vector3.zero;
        rotation = Vector3.zero;
        // Forwards/Backwards movement
        float movementValue = 0.0f;
        if (Input.GetAxis("Vertical") > 0.0f)
        {
            moveDir = transform.forward;
            movementValue = 1.0f;
        }
        else if(Input.GetAxis("Vertical") < 0.0f)
        {
            moveDir = -transform.forward;
            movementValue = -1.0f;
        }
        animator.SetFloat("LinearSpeed", movementValue);
        // Run or walk
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runningSpeed;
            animator.SetBool("isRunning", true);
        }
        else
        {
            currentSpeed = walkingSpeed;
            animator.SetBool("isRunning", false);
        }
        // Set rotation speed to idle rotation speed
        float rotationSpeed = idleRotationSpeed;
        // Change rotation speed if the player is not idle
        if(movementValue != 0.0f)
        {
            // Check if the current speed is running or walking speed
            if(currentSpeed == runningSpeed)
            {
                rotationSpeed = runningRotationSpeed;
            }
            else
            {
                rotationSpeed = walkingRotationSpeed;
            }
        }
        // Rotate right/left
        rotation.y = Input.GetAxis("Horizontal");
        rotation *= rotationSpeed;
        animator.SetFloat("AngularSpeed", rotation.y);
    }
     // Fixed update is called before physics calculations
    void FixedUpdate()
    {
        // Move rigid body
        body.MovePosition(body.position + moveDir * currentSpeed * Time.deltaTime);
        // Rotate rigid body
        Quaternion qRotation = Quaternion.Euler(rotation * Time.deltaTime);
        body.MoveRotation(body.rotation * qRotation);
    }
}
