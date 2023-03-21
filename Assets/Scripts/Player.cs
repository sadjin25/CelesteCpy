using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player player;

    private Animator anim;

    private enum SpriteState
    {
        // Start with 0
        idle, running, jumping, falling, dead
    }
    private SpriteState State = SpriteState.idle;


    public bool isDead
    {
        get;

        private set;
    }

    private bool isGameOver = false;

    public int MelonCnt;

    void Start()
    {
        // Singleton Init
        if (player == null)
        {
            player = this;
        }

        else
        {
            Destroy(gameObject);
        }

        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        DeadChk();
        AnimTransition();
    }

    public void GetMelon()
    {
        MelonCnt++;
    }

    private void DeadChk()
    {
        if (isDead && !isGameOver)
        {
            PlayerMovement.playerMovement.rb.velocity = Vector2.zero;
            isGameOver = true;

            GameManager.Instance.LoadMap();
        }
    }

    private void AnimTransition()
    {
        // Running
        if (Mathf.Abs(PlayerMovement.playerMovement.rb.velocity.x) > 0.1f)
        {
            State = SpriteState.running;
        }
        // Idle
        else
        {
            State = SpriteState.idle;
        }

        // Jumping
        if (PlayerMovement.playerMovement.IsJumping())
        {
            State = SpriteState.jumping;
        }

        // Falling
        else if (PlayerMovement.playerMovement.IsFalling())
        {
            State = SpriteState.falling;
        }

        if (isDead)
        {
            State = SpriteState.dead;
        }

        anim.SetInteger("State", (int)State);
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
