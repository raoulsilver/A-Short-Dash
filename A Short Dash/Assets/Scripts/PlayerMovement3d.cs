using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class PlayerMovement3d : MonoBehaviour
{
    [SerializeField]
    Material blueMaterial,redMateral;
    [SerializeField]
    GameObject hat;
    [SerializeField]
    GameObject after1Point,after2point;

        [SerializeField]
    float rotationSmooth;

    //current angle that we want the player to rotate to
    float currentAngle;

    [SerializeField]
    GameObject playerModel;
    Quaternion targetRotation;
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField, Range(0f,1f)] private float footstepVolume = 0.7f;
    private AudioSource footstepSource;

    public MovementState state;
    public bool frozen = false;

    Color custBlue = new Color(0.2233593f,0.2330129f,0.3702503f);
    Color custRed = new Color(0.7025235f,0.1595969f,0.1511034f);
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air,
        frozen,
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponentInChildren<Animator>();

        readyToJump = true;

        startYScale = transform.localScale.y;

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepClip;
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 0f;
        footstepSource.volume = footstepVolume;
        if (PlayerPrefs.GetInt("LevelJustFinished") == 1)
        {
            transform.position = after1Point.transform.position;
        }
        if (PlayerPrefs.GetInt("LevelJustFinished") == 2)
        {
            transform.position = after2point.transform.position;
        }
    }

    private void Update()
    {
        if(PlayerPrefs.GetInt("hasHat")==1){
            hat.SetActive(true);
        }
        else
        {
            hat.SetActive(false);
        }
        if (PlayerPrefs.GetInt("Feathers") <1)
        {
            blueMaterial.color=custBlue;
        }
        else
        {
            blueMaterial.color=custRed;
        }

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (!frozen) MyInput();
        if (frozen)
        {
            horizontalInput = 0; 
            verticalInput = 0;
        } 
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        bool isMoving = grounded && (horizontalInput != 0 || verticalInput != 0);
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }

        if (isMoving && !footstepSource.isPlaying && footstepClip != null)
        {
            footstepSource.Play();
        }
        else if (!isMoving && footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        if(horizontalInput!= 0 || verticalInput!=0)
        RotatePlayer();
    }

    private void RotatePlayer()
    {
        Vector3 movementInput = new Vector3(horizontalInput, 0, verticalInput).normalized;
        float target = Mathf.Atan2(movementInput.x, movementInput.z) * Mathf.Rad2Deg;
        playerModel.transform.rotation = Quaternion.Euler(0, target, 0);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        
        
        //currentAngle = Mathf.SmoothDampAngle(currentAngle, target, ref currentAngle, rotationSmooth);
        //playerModel.transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                //based on that rotation, find where we want to move to
        //Quaternion rotateMove = Quaternion.Euler(0, target, 0);
        //Debug.Log(currentAngle);
        //currentAngle = Mathf.SmoothDampAngle(playerModel.transform.rotation.y,target,ref rb.linearVelocity.y,rotationSmooth);
        
        //playerModel.transform.rotation = Quaternion.Euler(0, currentAngle, 0);
        //rb.MoveRotation(rotateMove * Time.deltaTime);

        //playerModel.transform.rotation = Quaternion.RotateTowards(playerModel.transform.rotation,targetRotation, 5 * Time.deltaTime);


        // when to jump
        /*if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }*/

        // start crouch
        /*if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }*/
    }

    private void StateHandler()
    {
        /*// Mode - Crouching
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }*/

        // Mode - Walking
        if (frozen)
        {
            state = MovementState.frozen;
        }
        if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //Debug.Log(angle);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}