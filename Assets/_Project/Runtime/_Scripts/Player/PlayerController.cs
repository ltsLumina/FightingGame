#region
#if UNITY_EDITOR
#endif
using System.Collections.Generic;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VInspector;
using static State;
#endregion

/// <summary>
/// The PlayerController is intended to be used as a base class for the player.
/// It is used to manage the individual player's input, movement, and state.
/// On the other hand, the PlayerManager is used to manage all players in the game including their settings, properties, and actions.
/// <seealso cref="PlayerManager"/>
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public partial class PlayerController : MonoBehaviour
{
    [Tab("Player Stats")]
    [SerializeField] float idleTimeThreshold;
    [SerializeField, ReadOnly] public float IdleTime; //TODO: Make this private, but I should probably move it to a different script.
    [SerializeField, ReadOnly] public float AirborneTime;

    [Header("Ground Check"), Tooltip("The minimum distance the ray-cast must be from the ground.")]
    [SerializeField] float raycastDistance = 1.022f;
    [SerializeField] LayerMask groundLayer;

    [Header("Player ID"), Tooltip("The player's ID. \"1\"refers to player 1, \"2\" refers to player 2.")]
    [SerializeField, ReadOnly] int playerID;
    
    [Tab("Movement")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float backwardSpeedFactor = 0.5f;
    [SerializeField] float acceleration = 8f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float velocityPower = 1.4f;

    [Tab("Settings")]
    [Header("Debug")]
    [SerializeField] bool godMode;
    [Space(5)]
    [Header("Player Components")]
    [SerializeField, ReadOnly] Healthbar healthbar;
    [SerializeField, ReadOnly] Animator animator;
    
    // Cached References
    PlayerController player;
    
    // Cached Animation References
    readonly static int Walk_Forward = Animator.StringToHash("Walk_Forward");
    readonly static int Walk_Backward = Animator.StringToHash("Walk_Backward");
    
    // -- Properties --
    
    public StateMachine StateMachine { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public InputManager InputManager { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    public HitBox HitBox { get; set; }
    public HurtBox HurtBox { get; set; }
    public bool IsInvincible { get; set; }

    string ThisPlayer => $"Player {PlayerID}";

    // -- Serialized Properties --

    public int PlayerID
    {
        get => playerID;
        private set => 
            // Clamp the playerID between 1 and 2.
            playerID = Mathf.Clamp(value, 1, 2);
    }
    
    public Healthbar Healthbar
    {
        get => healthbar;
        set => healthbar = value;
    }

    public Animator Animator
    {
        get => animator;
        set => animator = value;
    }

    void Awake()
    {
        player       = GetComponent<PlayerController>();
        StateMachine = GetComponent<StateMachine>();
        Rigidbody    = GetComponent<Rigidbody>();
        InputManager = GetComponentInChildren<InputManager>();
        PlayerInput  = GetComponentInChildren<PlayerInput>();
        HitBox       = GetComponentInChildren<HitBox>();
        HurtBox      = GetComponentInChildren<HurtBox>();
        Animator     = GetComponentInChildren<Animator>();
        
        // Enable the player input.
        DisablePlayer(false);
    }

    void OnDestroy() => Healthbar.OnPlayerDeath -= Death;

    // Rotate the player when spawning in to face in a direction that is more natural.
    void Start() => Initialize();

    void Update()
    {
        CheckIdle();
        
        RotateToFaceEnemy();

        //TODO: Temporary fix to test new state machine.
        if (StateMachine.CurrentState is not AttackState or AirborneAttackState)
        {
            if (IsGrounded())
            {
                // Reset the airborne time if the player is grounded.
                AirborneTime = 0;
                
                DEBUGMovement();
            }
            else
            {
                // If the player is airborne, we add to the airborne time.
                AirborneTime += Time.deltaTime;
                
            }
        }
    }

    void DEBUGMovement()
    {
        // Getting the move input from the player's input manager.
        Vector2 moveInput = InputManager.MoveInput;

        // Fix rigidbody velocity issue (velocity is absurdly low when standing still)
        if (moveInput == Vector2.zero) Rigidbody.velocity = new (0, Rigidbody.velocity.y);
        
        // Animating the player based on the move input.
        Animator.SetBool(Walk_Forward, moveInput.x  > 0);
        Animator.SetBool(Walk_Backward, moveInput.x < 0);

        // Determining the direction of the movement (left or right).
        int moveDirection = (int) moveInput.x;

        // Calculating the target speed based on direction and move speed.
        // If moving backward, multiply the moveSpeed with the backwardSpeedFactor
        float targetSpeed = moveDirection * (moveInput.x < 0 ? moveSpeed * backwardSpeedFactor : moveSpeed);

        // Calculate difference between target speed and current velocity.
        float speedDifference = targetSpeed - Rigidbody.velocity.x;

        // Determine the acceleration rate based on whether the target speed is greater than 0.01 or not.
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        // Calculate the final movement force to be applied on the player's rigidbody.
        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velocityPower) * Mathf.Sign(speedDifference);

        // Apply the force to the player's rigidbody.
        Rigidbody.AddForce(movement * Vector3.right);
    }
    
    /// <summary>
    ///     Initialize the player to the correct state.
    ///     This includes setting the player's color and spawn point as well as the player's ID.
    /// </summary>
    void Initialize()
    {
        PlayerManager.AddPlayer(this);
        
        PlayerID = PlayerInput.playerIndex + 1;
        gameObject.name = $"Player {PlayerID}";
        
        // Parenting the player to the header is purely for organizational purposes.
        const string headerTag = "[Header] Players";
        Transform header = GameObject.FindGameObjectWithTag(headerTag).transform;

        if (header == null)
        {
            Debug.LogError("Header not found. Please check if the tag is correct.");
            return;
        }

        transform.SetParent(header);

        var playerManager = PlayerManager.Instance;

        playerManager.SetPlayerSpawnPoint(this, PlayerID);
        PlayerManager.AssignHealthbarToPlayer(this, PlayerID);

        Healthbar.OnPlayerDeath += Death;
    }

    void RotateToFaceEnemy()
    {
        if (PlayerManager.OtherPlayer(player) == null) return;
        
        List<PlayerController> players        = PlayerManager.Players;
        PlayerController       oppositePlayer = players[PlayerID == 1 ? 1 : 0];
        Transform              model          = transform.GetComponentInChildren<Animator>().transform;

        Quaternion targetRotation;

        if (oppositePlayer.transform.position.x > transform.position.x)
        {
            model.localScale = new (1, 1, 1);
            targetRotation   = Quaternion.Euler(0, 70, 0);
        }
        else
        {
            model.localScale = new (-1, 1, 1);
            targetRotation   = Quaternion.Euler(0, -70, 0);
        }

        // Lerp rotation over time
        const float rotationSpeed = 0.75f; // Adjust this value to change the speed of rotation
        model.rotation = Quaternion.Lerp(model.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // -- State Checks --
    // Related: StateChecks.cs

    void CheckIdle()
    {
        // Check if the player is idle.
        if (IsIdle())
        {
            // If the player is idle, we add to the idle time.
            IdleTime += Time.deltaTime;

            // If the idle time is greater than the threshold, we transition to the idle state.
            if (IdleTime >= idleTimeThreshold)
            {
                StateMachine.TransitionToState(StateType.Idle);
            }
        }
        else { IdleTime = 0; }
    }

    public void DisablePlayer(bool disabled)
    {
        player.enabled       = !disabled;
        InputManager.Enabled = !disabled;
        HitBox.enabled       = !disabled;
        HurtBox.enabled      = !disabled;
    }

    void Death(PlayerController playerThatDied)
    {
        DisablePlayer(true);
        GamepadExtensions.RumbleAll(this);

        // Get the Volume component
        var volume = FindObjectOfType<Volume>();
        if (volume == null) return;

        DeathEffect();

        // Stop any ongoing animations and play the death animation
        Animator.Play("Idle");
        //Animator.SetTrigger("HasDied");
        
        Debug.Log($"{ThisPlayer} is dead!");

        return;
        void DeathEffect()
        {
            // Try to get the ChromaticAberration effect
            if (!volume.profile.TryGet(out ChromaticAberration chromaticAberration)) return;
            if (!volume.profile.TryGet(out DepthOfField depthOfField)) return;
            if (!volume.profile.TryGet(out LensDistortion lensDistortion)) return;
            
            Sequence sequence = DOTween.Sequence();
            int      mult     = 2;
            sequence.Join(DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, 1f     * mult, .65f).SetEase(Ease.InOutCubic));
            sequence.Join(DOTween.To(() => depthOfField.focusDistance.value, x => depthOfField.focusDistance.value       = x, 1.85f  * mult, .5f).SetEase(Ease.OutCubic));
            sequence.Join(DOTween.To(() => lensDistortion.intensity.value, x => lensDistortion.intensity.value           = x, -0.35f * mult, 2f).SetEase(Ease.OutCubic));
            //sequence.Append(DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, 1f, .65f).SetEase(Ease.OutCubic)); // Holds the effect for a bit
            sequence.AppendInterval(.1f); // Required to prevent the next sequence from starting too early
            sequence.Join(DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, 0f, .5f).SetEase(Ease.OutBounce));
            sequence.Join(DOTween.To(() => depthOfField.focusDistance.value, x => depthOfField.focusDistance.value       = x, 1.85f, .5f).SetEase(Ease.OutBounce));
            sequence.Join(DOTween.To(() => lensDistortion.intensity.value, x => lensDistortion.intensity.value           = x, 0f, .5f).SetEase(Ease.OutCubic));
            sequence.Play();
        }
    }
}

// #if UNITY_EDITOR
// [CustomEditor(typeof(PlayerController))]
// public class PlayerControllerInspector : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
//         
//         var player = (PlayerController) target;
//         var healthbar = player.Healthbar;
//         if (player == null || healthbar == null) return;
//
//         // Replace the health variable in the inspector with the healthbar's value
//         // so that the healthbar's value can be changed in the inspector.
//         player.health = healthbar.Value;
//     }
// }
// #endif