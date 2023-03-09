using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    [SerializeField] private Transform groundChkr;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallChkr;

    private enum SpriteState
    {
        // Start with 0
        idle, running, jumping, falling, dead
    }
    private SpriteState State = SpriteState.idle;

    private float xDirection;
    private float yDirection;
    private bool isFacingRight = true;

    private bool isGrounded;

    private bool isWallGrabbed;
    private short grabbedDirection;
    private float stamina;
    private readonly float maxClimbSpeed = 4f;
    private readonly float climbJumpStamina = 1.5f;

    private readonly float maxSpeed = 10f;
    private readonly float jumpTime = 0.15f;
    public float gravityMult = 1.5f;
    public float lowJumpMult = 1.5f;
    private float jumpForce = 14f;
    private bool isPressedJump;

    public bool isDead = false;
    public bool isGameOver = false;

    public int MelonCnt;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        DeadChk();
        AnimTransition();
        if (!isDead)
        {
            GroundChk();
            WallChk();
            JumpAndFallChk();

            if (!isWallGrabbed)
            {
                Flip();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isDead)
        {
            Movement();
            FollowerMovement();
        }
    }

    private void FollowerMovement()
    {

    }

    public void GetMelon()
    {
        MelonCnt++;
    }

    private void DeadChk()
    {
        if (isDead && !isGameOver)
        {
            rb.velocity = Vector2.zero;
            isGameOver = true;
            // TODO : make game manager, and load scene func goes in here.
            StartCoroutine(LoadNewScene());
        }
    }

    private IEnumerator LoadNewScene()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("basicScene1");
    }

    private void JumpAndFallChk()
    {
        // Jumping
        // TODO     : movement should be in FixedUpdate()
        // TODO     : add half grav threshold at the top of jump.

        if (!isWallGrabbed)
        {
            if (rb.velocity.y < 0 && !isGrounded)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * gravityMult * Time.deltaTime;
            }

            if (isPressedJump && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce); ;
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
                if (xDirection == grabbedDirection)
                {
                    stamina -= climbJumpStamina;
                    rb.velocity = new Vector2(0f, jumpForce);
                }

                else
                {
                    isWallGrabbed = false;
                    rb.velocity = new Vector2(xDirection * maxSpeed, jumpForce);
                }
            }
        }
    }

    private void Movement()
    {
        // Running
        if (!isWallGrabbed)
        {
            rb.velocity = new Vector2(xDirection * maxSpeed, rb.velocity.y);
        }

        // Grabbing Wall
        if (isWallGrabbed)
        {
            rb.velocity = new Vector2(rb.velocity.x, yDirection * maxClimbSpeed);
        }

    }

    private void AnimTransition()
    {
        // Running
        if (Mathf.Abs(xDirection) > 0f)
        {
            State = SpriteState.running;
        }
        // Idle
        else
        {
            State = SpriteState.idle;
        }

        // Jumping, fixing float err
        if (rb.velocity.y > 0.1f)
        {
            State = SpriteState.jumping;
        }

        // Falling, fixing float err
        else if (rb.velocity.y < -0.1f)
        {
            State = SpriteState.falling;
        }

        if (isDead)
        {
            State = SpriteState.dead;
        }

        anim.SetInteger("State", (int)State);
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


    private void GroundChk()
    {
        isGrounded = Physics2D.OverlapCircle(groundChkr.position, 0.2f, groundLayer);

        if (isGrounded)
        {
            // can hold wall for (x)f seconds.
            stamina = 7f;
        }
    }

    private void WallChk()
    {
        isWallGrabbed = Physics2D.OverlapCircle(wallChkr.position, 0.2f, groundLayer);

        if (isWallGrabbed)
        {
            // When Player Grab the wall first
            if (grabbedDirection == 0)
            {
                grabbedDirection = (short)xDirection;
                rb.velocity = Vector2.zero;
            }

            stamina -= Time.deltaTime;
        }
        else
        {
            grabbedDirection = 0;
        }

        if (stamina <= 0)
        {
            isWallGrabbed = false;
        }
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Collectible"))
        {
            Collectible pickupItem = collider.GetComponent<Collectible>();
            pickupItem.Pickup(gameObject.GetComponent<Player>());
        }

        else if (collider.gameObject.CompareTag("Trap"))
        {
            isDead = true;
        }
    }

}
