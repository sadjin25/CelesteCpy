using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melon : Collectible
{
    private readonly float speed = 1f;
    private readonly float length = 0.5f;
    private Vector2 firstPos;

    public override void Pickup(Player player)
    {
        player.GetMelon();
        Destroy(gameObject);
    }

    private void Start()
    {
        firstPos = gameObject.transform.position;
    }

    private void FixedUpdate()
    {
        float yCur = length * Mathf.Sin(Time.time * speed);
        gameObject.transform.position = new Vector2(firstPos.x, firstPos.y + yCur);
    }

}
