using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement playerMovement;
    public Rigidbody2D rb;

    [SerializeField] private Transform groundChkr;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallChkr;

    private float xDirection;
    private float yDirection;
    private bool isPressedGrab;
    public bool isPressedJump;

    private bool isFacingRight = true;

    private bool isGrounded;

    private bool isWallGrabbed;
    private short grabbedDirection;
    public float stamina;
    public float staminaMinusMult = 1f;
    private readonly float maxStamina = 7f;
    private readonly float maxClimbSpeed = 3f;
    private readonly float climbJumpStamina = 1.5f;
    private readonly float minStaminaToClimbJump = 2f;

    private readonly float maxSpeed = 10f;
    private readonly float noControlTime = 0.15f;
    private readonly float noControlTimeForStJump = 0.3f;
    public float noControlTimeCnt;


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
            NoControlTimeControl();
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

                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, maxFallVel, 0f));
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
                // Can't control the player until noControlTimeCnt passes 'noControlTime'
                isWallGrabbed = false;


                if (noControlTimeCnt < 0.1f)
                {
                    // Climb Jump
                    if (xDirection == grabbedDirection && stamina >= minStaminaToClimbJump)
                    {
                        noControlTimeCnt = noControlTimeForStJump;
                        stamina -= climbJumpStamina;
                        rb.velocity = new Vector2(0f, jumpForce);
                    }

                    //  Wall Jump
                    else
                    {
                        noControlTimeCnt = noControlTime;
                        rb.velocity = new Vector2(xDirection * maxSpeed, jumpForce);
                    }
                }
            }
        }
    }

    private void Movement()
    {
        // BUG : If horizontal key and vertical key are pressed both, velocity goes 10, 8 -> 7, 5 (?? : y value)   

        // Running
        if (!isWallGrabbed)
        {
            rb.velocity = new Vector2(xDirection * maxSpeed, rb.velocity.y);
        }

        // Wall Climbing
        else if (isWallGrabbed)
        {
            rb.velocity = new Vector2(rb.velocity.x, yDirection * maxClimbSpeed);

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
        isWallGrabbed = isPressedGrab && Physics2D.OverlapCircle(wallChkr.position, 0.2f, groundLayer);

        if (stamina <= 0 || isGrounded || noControlTimeCnt > 0.1f)
        {
            isWallGrabbed = false;
        }

        if (isWallGrabbed)
        {
            // Stick to wall.
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;

            // Grab the wall at first or didn't pressed arrow key.
            if (grabbedDirection == 0)
            {
                grabbedDirection = (short)xDirection;
            }
        }

        // Not Grabbing.(wall jumping / isGrounded, etc)
        else
        {
            grabbedDirection = 0;
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

    private void NoControlTimeControl()
    {
        if (noControlTimeCnt > 0f)
        {
            noControlTimeCnt -= Time.deltaTime;
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

        if (noControlTimeCnt > 0.1f)
        {
            xDirection = 0f;
            yDirection = 0f;
        }
    }

    public void GetInputJump(InputAction.CallbackContext context)
    {
        // MAYBE BUG? : It doesn't immediately change the value like FixedUpdate()... OR NOT

        if (context.performed)
        {
            isPressedJump = true;
        }

        if (context.canceled || noControlTimeCnt > 0.1f)
        {
            isPressedJump = false;
        }
    }

    public void GetInputGrab(InputAction.CallbackContext context)
    {
        // MAYBE BUG? : It doesn't immediately change the value like FixedUpdate()... OR NOT

        if (context.performed)
        {
            isPressedGrab = true;
        }

        if (context.canceled)
        {
            isPressedGrab = false;
        }
    }
}
