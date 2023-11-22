using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerVariables", menuName = "SOs/PlayerVariables")]
public class PlayerConstVariables : ScriptableObject
{
    //TODO: make all of vars const

    //------------------------CLIMB-----------------------------
    public float maxStamina = 7f;
    public float maxClimbSpeed = 3f;
    public float climbJumpStamina = 7 / 4f;
    public float minStaminaToClimbJump = 2f;

    //------------------------MOVE-----------------------------
    public float maxSpeed = 10f;
    // takes 6 frames to Max
    public float runAccTime = 6f;
    // takes 3 frames to 0
    public float runDecTime = 3f;

    public float gravityMult = 1.5f;

    //------------------------JUMP-----------------------------
    // WARNING : This value should be MINUS.
    public float maxFallVel = -20f;
    public float lowJumpMult = 1.5f;
    public float jumpForce = 14f;

    //------------------------DASH-----------------------------
    public float dashForce = 20f;
}
