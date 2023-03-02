using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundChk;
    [SerializeField] private LayerMask groundLayer;

    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    private bool isJumped = false;

    private readonly float coyoteTime = 0.2f;
    private float coyoteTimeCnt;


    void Update()
    {
        CoyoteTime();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        Flip();
    }


    private void CoyoteTime()
    {
        if (IsGrounded())
        {
            coyoteTimeCnt = coyoteTime;
        }

        else
        {
            coyoteTimeCnt -= Time.deltaTime;
        }
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && coyoteTimeCnt > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower * 0.5f);

            coyoteTimeCnt = 0f;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundChk.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (!isFacingRight && horizontal > 0f || isFacingRight && horizontal < 0f)
        {
            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

}
