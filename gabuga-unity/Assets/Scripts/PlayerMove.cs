using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MilkShake;

public class PlayerMove : MonoBehaviour
{
    #region Singleton
    public static PlayerMove Instance;
    void Awake()
    {
        Instance = this;
    }
    #endregion
    [Header("Assignables")]
    public Transform orientation;
    public Transform playerBody;
    public LayerMask playerLayerMask;

    [Header("Movement")]
    public bool canMove = true;
    public float moveSpeed;
    public float airSpeed;
    float horizontalMovement;
    float verticalMovement;


    [Header("Cam movement")]
    float mouseX;
    float mouseY;
    float xRotation;
    float yRotation;
    float multiplier = 0.1f;
    public bool CanLook = true;

    [Range(0, 100)]
    public float sens;

    [Header("Jumping")]
    public bool canJump;
    public float jumpCooldown; // the cooldown limit
    public float currentJumpCooldown = 0f; //the actual cooldown
    public float jumpForce;
    float playerHeight = 2f;
    float airTime;

    [Header("Ground Detection")]
    bool isGrounded;
    bool cancelGround;
    float surfaceAngle;
    float maxSlopeAngle = 60f;
    bool wishJump;
    Vector3 normalVector;

    [Header("Drag")]
    float groundDrag = 6f;
    float airDrag = 1f;

    [Header("Crouching & Sliding")]
    CapsuleCollider playerCol;
    bool underObstruction;

    [Header("Slope Stuff")]
    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    RaycastHit slopeHit;
    public Rigidbody rb;

    [Header("Camera")]
    public Transform camHolder;
    public float camTilt;
    public float camTiltTime;
    float camShakeValue;


    [Header("Footsteps")]
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;
    public AudioClip[] landingClips;
    public AudioClip slideClip;
    float baseStepSpeed = 0.3f;
    float footstepTimer = 0f;
    float GetCurrentOffset => baseStepSpeed;

    float extraGravityForce = -200f;

    public float tilt { get; private set; }

    private bool OnSlope()
    {
        if (isGrounded && Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 1f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCol = GetComponentInChildren<CapsuleCollider>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        MyInput();
        ControlDrag();
        CheckAirTime();
        //HandleFootsteps();
        Look();

        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

        underObstruction = Physics.Raycast(transform.position, Vector3.up, playerHeight / 2 + 0.15f);

        camShakeValue = rb.velocity.magnitude / 9f;
    }

    private void OnCollisionStay(Collision other)
    {

        //Loop through each contact point
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.GetContact(i).normal;

            if (IsFloor(normal))
            {
                isGrounded = true;
                cancelGround = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if (!cancelGround)
        {
            cancelGround = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    public bool IsFloor(Vector3 v)
    {
        surfaceAngle = Vector3.Angle(v, Vector3.up);
        return surfaceAngle < maxSlopeAngle;
    }

    private void StopGrounded()
    {
        isGrounded = false;
    }

    void FixedUpdate()
    {
        CameraTilting();
        if(canMove) MovePlayer();  

        if (wishJump)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
            wishJump = false;
        }

        rb.AddForce(Vector3.up * extraGravityForce, ForceMode.Force);
    }
    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;

        if (currentJumpCooldown >= jumpCooldown)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
            currentJumpCooldown += Time.deltaTime;
            currentJumpCooldown = Mathf.Clamp(currentJumpCooldown, 0f, jumpCooldown);
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded && canJump && canMove)
        {
            Jump();
            currentJumpCooldown = 0f;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCrouch();
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            StopCrouch();
        }
    }

    void MovePlayer()
    {
        if (isGrounded)
        {
            if (OnSlope())
            {
                rb.AddForce(slopeMoveDirection.normalized * moveSpeed, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Acceleration);
            }
        }
        else
        {
            rb.AddForce(moveDirection.normalized * airSpeed, ForceMode.Acceleration);
        }
    }

    void Jump()
    {
        wishJump = true;
    }

    void StartCrouch()
    {
        playerBody.localScale = new Vector3(1, 0.65f, 1);
        playerBody.position = new Vector3(playerBody.position.x, playerBody.position.y - 0.35f, playerBody.position.z);
    }

    void StopCrouch()
    {
        playerBody.localScale = new Vector3(1, 1f, 1);
        playerBody.position = new Vector3(playerBody.position.x, playerBody.position.y + 0.35f, playerBody.position.z);
        if (!canMove) canMove = true;
    }

    void Look()
    {
        if (CanLook)
        {
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");

            yRotation += mouseX * sens * multiplier;
            xRotation -= mouseY * sens * multiplier;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            camHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, tilt);
            orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }

        if (!isGrounded)
        {
            rb.drag = airDrag;
        }
    }

    void CheckAirTime()
    {
        if (isGrounded || OnSlope())
        {
            airTime = 0f;
        }
        else
        {
            airTime += Time.deltaTime;
        }
    }

   /* void HandleFootsteps()
    {
        if (!isGrounded || isSliding || rb.velocity.magnitude <= 0) return;

        footstepTimer -= Time.deltaTime;

        if (rb.velocity.magnitude >= 6 && footstepTimer <= 0 && isGrounded)
        {
            footstepAudioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length - 1)]);

            footstepTimer = GetCurrentOffset;
        }
    } */

    void CameraTilting()
    {
        if (Input.GetKey(KeyCode.A)) tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime / 2);
        else tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime / 2);
        else tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);

    }

}
