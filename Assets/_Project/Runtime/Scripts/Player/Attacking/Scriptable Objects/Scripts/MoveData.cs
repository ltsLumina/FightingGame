﻿using UnityEngine;

/// <summary>
/// This is a ScriptableObject that represents a move (e.g. punch, kick, etc.) that can be used by the player.
/// It is used by the Moveset ScriptableObject where each move is stored in an array.
/// This ScriptableObject is created by the MovesetCreator.cs EditorWindow.
/// </summary>
[CreateAssetMenu(fileName = "Move", menuName = "(Debug) Create Move", order = 0)]
public class MoveData : ScriptableObject
{
    public enum Type
    {
        Punch,
        Kick,
        Slash,
        Unique
    }

    /// <summary>
    /// The direction that the player needs to move in order to perform this move.
    /// <para>For instance, if a move is set to "Forward", then the player needs to press the forward key in order to perform the move.</para>
    /// <para>Or if a move is set to "Neutral", then the player needs to press the down key in order to perform the move.</para>
    /// An example of a move that is set to "Down" is a crouching attack or sweep. (2P, 2K, 2S, 2U)
    /// </summary>
    public enum Direction
    {
        Neutral, // Example: 5P, 5K, 5S, 5U
        Forward, // Example: 6P, 6K, 6S, 6U
        Down,    // Example: 2P, 2K, 2S, 2U
        Up       // Example: 8P, 8K, 8S, 8U
    }
    
    /// <summary>
    /// The guard height that an enemy needs to be in order to be block this move.
    /// <para>For instance, if a move is set to "Low", then the enemy must be blocking low (crouch-blocking) in order to block the move,
    /// and if they are blocking high (standing-blocking), then they will get hit by the move.</para>
    /// If a move is set to "All", then the enemy can block the move regardless of their guard height.
    /// </summary>
    public enum Guard
    {
        High,
        Low,
        All
    }
    
    public Type type;
    public Direction direction;
    public Guard guard;

    [Space(10)]
    [Header("Resources")]
    public AnimationClip animation;
    public AudioClip audioClip;
    public Sprite sprite;
    
    [Space(15)]
    [Header("Move Attributes")]
    
    [Tooltip("Name of the move.")]
    new public string name;

    [Tooltip("Description of the move.")]
    public string description;
    
    [Tooltip("Damage caused by the move.")]
    public float damage;

    [Tooltip("Number of frames between inputting an attack and when the attack becomes active. Includes first active frame.")]
    public float startup;

    [Tooltip("Number of frames for which a move has hitboxes. Occurs after Startup.")]
    public float active;

    [Tooltip("Number of frames after a move's active frames during which the character cannot act assuming the move is not canceled.")]
    public float recovery;

    [Tooltip("Block advantage/disadvantage.")]
    public float blockstun;

    [Space(15)]
    [Header("Move Properties")]

    [Tooltip("Whether the move can be performed while airborne.")]
    public bool isAirborne;

    [Tooltip("Whether the move hits low and causes knockdown.")]
    public bool isSweep;

    [Tooltip("Whether the move hits crouching opponents.")]
    public bool isOverhead;

    [Tooltip("Whether the move can be blocked.")]
    public bool isArmor;

    [Tooltip("Whether the player becomes invincible to attacks while performing the move.")]
    public bool isInvincible;

    [Tooltip("Whether the move ignores the enemy's guard.")]
    public bool isGuardBreak;
    
}
