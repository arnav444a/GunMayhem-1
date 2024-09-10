using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerStateMachine.State
{
    PlayerStateMachine player;
    public override PlayerState type => PlayerState.Falling;
    public PlayerFallState(PlayerStateMachine Player) : base(Player)
    {
        player = Player;
    }
    public override void OnStateUpdate()
    {
        if (player.IsGrounded())
        {
            player.ChangeState(PlayerState.Moving);
        }
        player.PlayerMovement();

    }
    public override void OnStateFixedUpdate()
    {
        if (player.rb.velocity.y < 0)
        {
            player.rb.gravityScale += player.gravityDownwardModifier * Time.deltaTime;
        }
        if (Mathf.Abs(player.rb.velocity.y) < 2f && Mathf.Sign(player.rb.velocity.y) == -1f)
        {
            player.movementMultiplier = player.jumpPeekMovementMultiplier;
        }
        else
        {
            player.movementMultiplier = player.jumpMovementMultiplier;
        }
    }
}
