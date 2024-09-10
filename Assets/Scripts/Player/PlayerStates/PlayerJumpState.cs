using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerStateMachine.State
{
    PlayerStateMachine player;
    public PlayerJumpState(PlayerStateMachine Player) : base(Player)
    {
        player = Player;
    }

    public override PlayerState type => PlayerState.Jumping;

    // Update is called once per frame
    public override void OnStateEnter(PlayerState prevStateType, object[] args)
    {
        player.rb.velocity = new Vector3(player.rb.velocity.x, 10);

    }
    public override void OnStateUpdate()
    {
        player.PlayerMovement();
    }
    public override void OnStateFixedUpdate()
    {
        if (player.rb.velocity.y > 0)
        {
            player.rb.gravityScale += player.gravityUpwardModifier * Time.deltaTime;
        }
        else if (player.rb.velocity.y < 0)
        {
            player.ChangeState(PlayerState.Falling);
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
