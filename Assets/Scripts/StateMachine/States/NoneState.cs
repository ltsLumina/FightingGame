﻿using UnityEngine;

public class NoneState : State
{
    public override StateType Type => StateType.Idle;
    public override int Priority => statePriorities[Type];

    public NoneState(PlayerController player) : base(player)
    { }

    public override void OnEnter()
    {
        Debug.Log("'None' state selected. This state is used when there is no state to transition to, or there is no player.");
        player.GetComponentInChildren<SpriteRenderer>().color = Color.grey;
    }

    public override void UpdateState()
    {

    }

    public override void OnExit()
    {

    }
}