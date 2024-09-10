using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementState : PlayerStateMachine.State
{
    PlayerStateMachine player;
    public override PlayerState type => PlayerState.Moving;
    public PlayerMovementState(PlayerStateMachine Player) : base(Player)
    {
        player = Player;
    }
    public override void OnStateUpdate()
    {
        player.PlayerMovement();
    }
    public override void OnStateEnter(PlayerState prevStateType, object[] args)
    {
        Debug.Log("Now Moving");
        player.rb.gravityScale = 1;
    }
}
