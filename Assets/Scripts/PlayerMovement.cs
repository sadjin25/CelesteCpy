using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement playerMovement;
    public Rigidbody2D rb;
    private Collider2D playerCollider;

    [SerializeField] private Transform groundChkr;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallChkr;
    private Vector2 wallChkrVec = new Vector2(0.2f, 1.5f);

    private float xDirection;
    private float yDirection;
    public bool isPressedGrab;
    public bool isPressedJump;

    private bool isFacingRight = true;

    private bool isGrounded;

    private bool isWallGrabbed;
    public float stamina;
    public float staminaMinusMult = 1f;
    public bool noGrab;
    private readonly float maxStamina = 7f;
    private readonly float maxClimbSpeed = 3f;
    private readonly float climbJumpStamina = 1.5f;
    private readonly float minStaminaToClimbJump = 2f;
    private readonly float noGrabTime = 0.4f;

    private readonly float maxSpeed = 10f;
    // takes 6 frames to Max
    private readonly float runAccTime = 6f;
    // takes 3 frames to 0
    private readonly float runDecTime = 3f;


    // DANGER : This is NOT a READ ONLY.
    private float playerDefaultGravity;
    private float gravityMult = 1.5f;
    // DANGER : This value should be MINUS.
    public readonly float maxFallVel = -20f;
    private float lowJumpMult = 1.5f;
    private float jumpForce = 14f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerDefaultGravity = rb.gravityScale;

        // Singleton Init
        if (playerMovement == null)
        {
            playerMovement = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!Player.player.isDead)
        {
            GroundChk();
            WallChk();
            JumpChk();
            FallChk();
            StaminaControl();
            Movement();

            if (!isWallGrabbed)
            {
                Flip();
            }
        }
    }

    private void FallChk()
    {
        if (!isWallGrabbed)
        {
            if (rb.velocity.y < 0 && !isGrounded)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * gravityMult * Time.deltaTime;

                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, maxFallVel));
            }
        }
    }


    private void JumpChk()
    {
        // Jumping
        // TODO     : add half grav threshold at the top of jump.
        // TODO     : seperate jump / fall(or grav) control

        if (!isWallGrabbed)
        {
            if (isPressedJump && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }

            if (!isPressedJump && rb.velocity.y > 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * lowJumpMult * Time.deltaTime;
            }
        }

        else if (isWallGrabbed)
        {
            if (isPressedJump)
            {
                isWallGrabbed = false;
                /*
                    // Straight Jump
                    if (xDirection == 0f && stamina >= minStaminaToClimbJump)
                    {
                        StartCoroutine(StopGrab(noGrabTime));
                        stamina -= climbJumpStamina;
                        rb.velocity = new Vector2(0f, jumpForce);
                    }
      */
                //  Wall Jump
                rb.velocity = new Vector2(xDirection * maxSpeed, jumpForce);
            }
        }

    }

    private void Movement()
    {
        // BUG : If horizontal key and vertical key are pressed both, velocity goes 10, 8 -> 7, 5 (?? : y value)   

        // Running
        if (!isWallGrabbed)
        {
            float addX = rb.velocity.x + xDirection * 1 / runAccTime * maxSpeed;
            rb.velocity = new Vector2(addX, rb.velocity.y);

            rb.velocity = new Vector2(Mathf.Min(Mathf.Abs(rb.velocity.x), maxSpeed * Mathf.Abs(xDirection) * Mathf.Sign(rb.velocity.x)), rb.velocity.y);

            // Deceleration when Release the key or direction change.
            if (xDirection == 0f || System.Math.Sign(xDirection) != System.Math.Sign(rb.velocity.x))
            {
                addX = rb.velocity.x + xDirection * 1 / runDecTime * maxSpeed;
                rb.velocity = new Vector2(addX, rb.velocity.y);

                rb.velocity = new Vector2(Mathf.Max(Mathf.Abs(rb.velocity.x), 0f), rb.velocity.y);
            }
        }

        // Wall Climbing
        else if (isWallGrabbed)
        {
            rb.velocity = new Vector2(0f, yDirection * maxClimbSpeed);

            // Climbing up the wall reduces stamina fast.
            if (yDirection > 0)
            {
                staminaMinusMult = 2f;
            }
            else
            {
                staminaMinusMult = 1f;
            }
        }

    }


    private void GroundChk()
    {
        isGrounded = Physics2D.OverlapCircle(groundChkr.position, 0.2f, groundLayer);

        if (isGrounded)
        {
            // can hold wall for (x)f seconds.
            // TODO : make Refill() func.
            stamina = maxStamina;
        }
    }

    private void WallChk()
    {
        isWallGrabbed = isPressedGrab && Physics2D.OverlapBox(wallChkr.position, wallChkrVec, 0f, groundLayer);


        if (stamina <= 0f || isGrounded)
        {
            isWallGrabbed = false;
        }

        if (isWallGrabbed)
        {
            // Stick to wall.
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // Not Grabbing.(wall jumping / isGrounded, etc)
        else
        {
            rb.gravityScale = playerDefaultGravity;
        }
    }

    private void StaminaControl()
    {
        if (isWallGrabbed)
        {
            stamina -= staminaMinusMult * Time.deltaTime;
        }
    }

    public bool IsJumping()
    {
        return rb.velocity.y > 0.1f;
    }

    public bool IsFalling()
    {
        return !isGrounded && rb.velocity.y < -0.1f;
    }

    private void Flip()
    {
        if (!isFacingRight && xDirection > 0f || isFacingRight && xDirection < 0f)
        {
            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public void GetInputDirection(InputAction.CallbackContext context)
    {
        xDirection = context.ReadValue<Vector2>().x;
        yDirection = context.ReadValue<Vector2>().y;
    }

    public void GetInputJump(InputAction.CallbackContext context)
    {
        // MAYBE BUG? : It doesn't immediately change the value like FixedUpdate()... OR NOT

        if (context.performed)
        {
            isPressedJump = true;
        }

        if (context.canceled)
        {
            isPressedJump = false;
        }
    }

    public void GetInputGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isPressedGrab = true;
        }

        if (context.canceled || noGrab)
        {
            isPressedGrab = false;
        }
    }

    private IEnumerator StopGrab(float stopTime)
    {
        noGrab = true;
        yield return new WaitForSeconds(stopTime);
        noGrab = false;
    }

}
