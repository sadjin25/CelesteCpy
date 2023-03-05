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

    private float direction;
    private float maxSpeed = 10f;
    private float jumpForce = 16f;
    private bool isFacingRight = true;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        GroundChk();
        Flip();
        AnimTransition();
    }

    void FixedUpdate()
    {
        Movement();
    }


    private void Movement()
    {
        //Running
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

        // Jumping
        if (rb.velocity.y > 0.1f)
        {
            State = SpriteState.jumping;
        }

        // Falling
        else if (rb.velocity.y < -0.1f)
        {
            State = SpriteState.falling;
        }

        anim.SetInteger("State", (int)State);
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            isGrounded = false;

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        //BUG : while goes up, we release the button again n again, it goes up by jumppower * 0.5f.
        if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * 0.5f);
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

}
