﻿#region
using Lumina.Debugging;
using UnityEngine;
#endregion

/*----------------------------------------------------------------------------
 * IMPORTANT NOTE: Use Time.fixedDeltaTime instead of Time.deltaTime
 *----------------------------------------------------------------------------*/

/// <summary>
/// <seealso cref="AttackStateData"/>
/// </summary>
public class AttackState : State
{
    // -- Abstract Variables --
    public override StateType Type => StateType.Attack;
    public override int Priority => statePriorities[Type];
    
    public bool IsAttacking { get; private set; }

    // -- State Specific Variables --
    float groundedAttackTimer;

    readonly AttackHandler attackHandler;
    readonly Animator animator;
    readonly Moveset moveset; // Could be declared as a local variable, but it will (probably) be used in the future.
    
    float attackAnimationLength;

      // -- Constructor --
      public AttackState(PlayerController player, AttackStateData stateData) : base(player)
      {
          animator = player.GetComponentInChildren<Animator>();
          moveset  = stateData.Moveset;

          Debug.Assert(moveset != null, "Moveset is null in the AttackStateData. Please assign it in the inspector.");

          attackHandler = new (moveset, animator, player);
      }

    public override bool CanBeInterrupted() => interruptibilityRules[Type];

    public override void OnEnter()
    {
        // Play the attack animation.
        IsAttacking = true;

        player.GetComponentInChildren<SpriteRenderer>().color = Color.red;

        groundedAttackTimer = 0;

        InputManager.AttackType attackType = player.InputManager.LastAttackPressed;

        if (attackType != InputManager.AttackType.None)
        {
            if (attackHandler.SelectAttack(attackType)) player.InputManager.LastAttackPressed = InputManager.AttackType.None; // Reset after usage
            else OnExit();
        }
        else
        {
            Debug.LogWarning("No attack type was selected. Something went wrong.");
        }
    }

    public override void UpdateState()
    {
        // Have to set the attack animation length here because the animator will return the length of the idle animation instead of the attack animation.
        // So we have to set the attack animation length every frame to ensure that it is the correct value.
        attackAnimationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        
        // Bug: On occasion, the animator will return the length of the idle animation instead of the attack animation.
        // If the attack animation length is 0.1, then the player GetCurrentAnimatorStateInfo(0) method has returned the value of the Idle animation.
        // TODO: I think I found a solution. Set the Exit Time in the Animator to 1 on the attack animation.
        
        // If the attack duration has not been reached, continue attacking.
        groundedAttackTimer += Time.deltaTime;

        if (player.IsGrounded()) { FGDebugger.Debug("Attacking on the ground!", LogType.Log, StateType.Attack); }

        // Player has become airborne after starting the attack, cancel the attack. (e.g. player gets knocked into the air) 
        else
        {
            FGDebugger.Debug("Player has been knocked into the air! \nCancelling the attack...", LogType.Log, StateType.Attack);

            // Cancel the attack.
            OnExit();
        }

        // i figured it out.
        // attackAnimationLength is the length of the IDLE ANIMATION, not the attack animation. :'(
        if (groundedAttackTimer >= attackAnimationLength)
        {
            OnExit();
        }
    }

    public override void OnExit()
    {
        // Cancel the attack animation by playing the idle animation.
        animator.Play("Idle");

        if (player.InputManager.MoveInput.x != 0 && player.IsGrounded())
            // If the player is moving, transition to the walk state.
            player.StateMachine.TransitionToState(StateType.Walk);
        
        else // If the player is not moving, transition to the idle state. 
            player.StateMachine.TransitionToState(StateType.Idle);

        IsAttacking = false;
    }
}