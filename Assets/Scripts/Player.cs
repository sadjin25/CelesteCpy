using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Transform groundChkr;
    [SerializeField] private LayerMask groundLayer;

    private float direction;
    private float maxSpeed = 10f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    public bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GroundChk();
        Flip();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(direction * maxSpeed, rb.velocity.y);
    }




    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            isGrounded = false;

            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        //BUG : while goes up, we release the button again n again, it goes up by jumppower * 0.5f.
        if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower * 0.5f);
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
