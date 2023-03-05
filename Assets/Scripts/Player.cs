using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    [SerializeField] private Transform groundChkr;
    [SerializeField] private LayerMask groundLayer;

    private enum SpriteState
    {
        // Start with 0
        idle, running, jumping, falling
    }
    private SpriteState State = SpriteState.idle;

    private readonly float maxSpeed = 10f;
    private float direction;
    private float jumpForce = 10f;
    private bool isFacingRight = true;
    public bool isGrounded;

    [SerializeField] public float jumpTime = 0.15f;
    [SerializeField] public float jumpTimeCnt;
    public bool isPressedJump;
    public bool isJumped;

    public int MelonCnt;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        JumpChk();
        GroundChk();
        Flip();
        AnimTransition();
    }

    void FixedUpdate()
    {
        Movement();
    }


    public void GetMelon()
    {
        MelonCnt++;
    }

    private void JumpChk()
    {
        // Jumping
        // TODO     : movement should be in FixedUpdate()
        // TODO2    : Player is kinda floating in the air. need to fix gravity.
        if (isPressedJump && isGrounded)
        {
            isJumped = true;
            jumpTimeCnt = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (isPressedJump && isJumped)
        {
            if (jumpTimeCnt > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCnt -= Time.deltaTime;
            }
            else
            {
                isJumped = false;
            }
        }
    }

    private void Movement()
    {
        // Running
        rb.velocity = new Vector2(direction * maxSpeed, rb.velocity.y);

    }

    private void AnimTransition()
    {
        //BUG : while in idle state, it doesn't instantly changed to jumping state. 
        // Running
        if (Mathf.Abs(direction) > 0f)
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

        anim.SetInteger("State", (int)State);
    }


    public void GetInputJump(InputAction.CallbackContext context)
    {
        // BUG : It doesn't immediately change the value like FixedUpdate() << Infinity Jump!

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
    }

    private void Flip()
    {
        if (!isFacingRight && direction > 0f || isFacingRight && direction < 0f)
        {
            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public void GetInputDirection(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>().x;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Collectible pickupItem = collider.GetComponent<Collectible>();

        pickupItem.Pickup(gameObject.GetComponent<Player>());
    }

}
