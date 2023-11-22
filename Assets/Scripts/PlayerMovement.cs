using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] InputReader _inputReader;

    public static PlayerMovement playerMovement;
    public Rigidbody2D rb;
    private Collider2D playerCollider;

    [SerializeField] private Transform groundChkr;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallChkr;
    private Vector2 wallChkrVec = new Vector2(0.2f, 1.5f);

    public bool isPlayerTimeStopped;

    //------------------INPUTS--------------------------
    private Vector2 _moveDir;
    public bool _grabInput;
    public bool _jumpInput;
    public bool _dashInput;
    //-------------------STATES-------------------------

    private bool isFacingRight = true;

    private bool isGrounded;

    private bool isWallGrabbed;
    private bool isWallClose;
    private bool isWallJumping;
    private bool isClimbJumping;
    public float stamina;
    public float staminaMinusMult = 1f;
    private readonly float maxStamina = 7f;
    private readonly float maxClimbSpeed = 3f;
    private readonly float climbJumpStamina = 7 / 4f;
    private readonly float minStaminaToClimbJump = 2f;

    private readonly float maxSpeed = 10f;
    // takes 6 frames to Max
    private readonly float runAccTime = 6f;
    // takes 3 frames to 0
    private readonly float runDecTime = 3f;


    private bool isDashing;
    private bool isDashAvailable;
    private readonly float dashForce = 20f;

    // DANGER : This is NOT a READ ONLY.
    private float playerDefaultGravity;
    private float gravityMult = 1.5f;
    // DANGER : This value should be MINUS.
    public readonly float maxFallVel = -20f;
    private float lowJumpMult = 1.5f;
    private float jumpForce = 14f;

    void OnEnable()
    {
        _inputReader.MoveEvent += OnMove;
        _inputReader.JumpEvent += OnJump;
        _inputReader.DashEvent += OnDash;
        _inputReader.GrabEvent += OnGrab;
    }

    void OnDisable()
    {
        _inputReader.MoveEvent -= OnMove;
        _inputReader.JumpEvent -= OnJump;
        _inputReader.DashEvent -= OnDash;
        _inputReader.GrabEvent -= OnGrab;
    }

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
        if (!Player.player.isDead && !isPlayerTimeStopped)
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
        if (isDashing)
        {
            return;
        }

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
        if (isDashing)
        {
            return;
        }


        if (!isWallGrabbed && !isWallJumping)
        {
            if (_jumpInput && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }

            else if (!_jumpInput && rb.velocity.y > 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * lowJumpMult * Time.deltaTime;
            }

            else if (Mathf.Abs(_moveDir.x) > 0f && _jumpInput && isWallClose && !isGrounded)
            {
                StartCoroutine(WallJump());
            }
        }

        else if (isWallGrabbed && _jumpInput && stamina > minStaminaToClimbJump)
        {
            StartCoroutine(ClimbJump());
        }
    }

    private void Movement()
    {
        // BUG? : xDir, yDir calculates diagonal value. 

        // Dashing
        if (_dashInput && isDashAvailable)
        {
            StartCoroutine(Dash());
        }

        // Running
        else if (!isWallGrabbed && !isDashing && !isWallJumping && !isClimbJumping)
        {
            float addX = rb.velocity.x + _moveDir.x * 1 / runAccTime * maxSpeed;
            rb.velocity = new Vector2(addX, rb.velocity.y);

            rb.velocity = new Vector2(Mathf.Min(Mathf.Abs(rb.velocity.x), maxSpeed * Mathf.Abs(_moveDir.x) * Mathf.Sign(rb.velocity.x)), rb.velocity.y);

            // Deceleration when Release the key or direction change.
            if (_moveDir.x == 0f || System.Math.Sign(_moveDir.x) != System.Math.Sign(rb.velocity.x))
            {
                addX = rb.velocity.x + _moveDir.x * 1 / runDecTime * maxSpeed;
                rb.velocity = new Vector2(addX, rb.velocity.y);

                rb.velocity = new Vector2(Mathf.Max(Mathf.Abs(rb.velocity.x), 0f), rb.velocity.y);
            }
        }

        // Wall Climbing
        else if (isWallGrabbed)
        {
            rb.velocity = new Vector2(0f, _moveDir.y * maxClimbSpeed);

            // Climbing up the wall reduces stamina fast.
            if (_moveDir.y > 0)
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
            isDashAvailable = true;
        }
    }

    private void WallChk()
    {
        isWallClose = Physics2D.OverlapBox(wallChkr.position, wallChkrVec, 0f, groundLayer);
        isWallGrabbed = _grabInput && isWallClose;

        if (isWallJumping)
        {
            isWallGrabbed = false;
            return;
        }

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
        if (!isFacingRight && _moveDir.x > 0f || isFacingRight && _moveDir.x < 0f)
        {
            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator WallJump()
    {
        rb.velocity = Vector2.zero;
        isWallJumping = true;

        Vector2 jumpVec = new Vector2(maxSpeed, jumpForce);

        if (isFacingRight)
        {
            jumpVec.x *= -1;

            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
        else
        {
            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
        rb.velocity = jumpVec;

        int frame = 10;
        while (frame-- > 0)
        {
            yield return null;
        }

        isWallJumping = false;
    }

    private IEnumerator Dash()
    {
        _dashInput = false;
        isDashAvailable = false;
        isDashing = true;

        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        // Wait for Input
        int frame = 3;
        while (frame-- > 0)
        {
            yield return null;
        }

        float X = _moveDir.x;
        float Y = _moveDir.y;
        float diagDashMult = 1f;
        if (Mathf.Abs(X) > 0f && Mathf.Abs(Y) > 0f)
        {
            diagDashMult = 0.707f;
        }
        rb.velocity = new Vector2(X * dashForce * diagDashMult, Y * dashForce * diagDashMult);

        // Dashing for 12 frames.
        frame = 12;
        while (frame-- > 0)
        {
            yield return null;
        }

        rb.gravityScale = playerDefaultGravity;
        isDashing = false;
    }

    private IEnumerator ClimbJump()
    {
        stamina -= climbJumpStamina;
        isClimbJumping = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = playerDefaultGravity;

        rb.velocity = new Vector2(0f, jumpForce);

        while (!IsFalling())
        {
            yield return null;
        }

        isClimbJumping = false;
    }

    #region INPUT EVENTS
    void OnMove(Vector2 input)
    {
        _moveDir = input;
    }

    void OnJump(bool isPressed) => _jumpInput = isPressed;

    void OnDash(bool isPressed) => _dashInput = isPressed;

    void OnGrab(bool isPressed) => _grabInput = isPressed;
    #endregion
}
