﻿#region
using UnityEngine;
#endregion

/*----------------------------------------------------------------------------
 * IMPORTANT NOTE: Use Time.fixedDeltaTime instead of Time.deltaTime
 *----------------------------------------------------------------------------*/
public class FallState : State
{
    public override StateType Type => StateType.Fall;
    public override int Priority => statePriorities[Type];

    public bool IsFalling { get; private set; }

    // -- State Specific Variables --
    float fallGravityMultiplier;
    float jumpHaltForce;

    // -- Constructor --
    public FallState(PlayerController player, FallStateData stateData) : base(player)
    {
        fallGravityMultiplier = stateData.FallGravityMultiplier;
        jumpHaltForce         = stateData.JumpHaltForce;
    }

    public override void OnEnter()
    {
        IsFalling = true;
        
        player.GetComponentInChildren<SpriteRenderer>().color = new (1f, 0.58f, 0f);
    }

    public override void UpdateState()
    {
        if (player.IsGrounded())
        {
            OnExit();
            return;
        }

        // Apply gravity
        if (player.Rigidbody.velocity.y < 0) 
            player.Rigidbody.AddForce(fallGravityMultiplier * Vector3.down);

        // Apply jump halt force
        if (player.Rigidbody.velocity.y > 0) 
            player.Rigidbody.AddForce(jumpHaltForce * Vector3.down);

        player.Animator.SetBool("IsFalling", IsFalling);
    }

    public override void OnExit()
    {
        // Perform any necessary cleanup or exit actions

        if (player.IsGrounded() && player.InputManager.MoveInput.x != 0) player.StateMachine.TransitionToState(StateType.Walk);
        else if (player.IsGrounded()) player.StateMachine.TransitionToState(StateType.Idle);

        // Play land animation.
        IsFalling = false;
        player.Animator.SetBool("IsFalling", IsFalling);
        player.Animator.SetTrigger("Land");
    }
}
