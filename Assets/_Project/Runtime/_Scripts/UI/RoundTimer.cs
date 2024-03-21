#region
using System.Collections.Generic;
using System.Globalization;
using Lumina.Essentials.Attributes;
using TMPro;
using UnityEngine;
#endregion

public class RoundTimer : MonoBehaviour
{
    enum TimerFormat
    {
        Whole,
        TenthDecimal,
        HundredthsDecimal,
    }

    [Header("Reference"), Space(10), ReadOnly]
    [SerializeField] TextMeshProUGUI timerText;
    
    [Header("Timer Settings")]
    [SerializeField, ReadOnly] float currentTime;
    [SerializeField, ReadOnly] bool finished;
    [SerializeField] bool countdownMode;
    
    [Header("Limit Settings"), Tooltip("If true, the timer will have a time limit.")]
    [SerializeField] bool hasTimeLimit;
    [SerializeField] float timeLimit;
    
    [Header("Format Settings")]
    [SerializeField, Tooltip("If true, the timer will use a custom format.")] 
    bool customFormat;
    [SerializeField, Tooltip("The value at which the timer will switch to a red color.")]
    float colorSwitchValue;
    [SerializeField, Tooltip("The value at which the timer will switch to a whole number format.")] 
    float tenthSwitchValue;
    [SerializeField, Tooltip("The value at which the timer will switch to a tenth decimal format.")]
    float hundredthsSwitchValue;

    public delegate void TimerEnded();
    public static event TimerEnded OnTimerEnded;
    
    // Cached References
    readonly Dictionary<TimerFormat, string> timeFormats = new ();
    
    // -- Properties --
    public float CurrentTime
    {
        get => currentTime;
        set => currentTime = value;
    }
    
    public bool Finished
    {
        get => finished;
        set => finished = value;
    }

    void Awake()
    {
        InitializeTimeFormats();
        FindTimerTextComponent();

        return;
        void FindTimerTextComponent()
        {
            var timerTextObject = GameObject.FindWithTag("[Header] User Interface").GetComponentInChildren<TextMeshProUGUI>();
            if (timerTextObject != null) timerText = timerTextObject.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void Start() => Finished = false;

    void OnEnable() => OnTimerEnded += TimerFinishedEvent;
    void OnDisable() => OnTimerEnded -= TimerFinishedEvent;

    void TimerFinishedEvent()
    {
        Finished = true;
        Debug.Log("Timer has ended!");
    }

    void Update()
    {
        if (timerText == null) return;

        if (countdownMode) DecreaseTime(Time.deltaTime);
        else IncreaseTime(Time.deltaTime);

        if (CurrentTime <= 0f)
        {
            Finished = true;
        }
        
        UpdateTimerText();
    }

    void InitializeTimeFormats()
    {
        timeFormats.Add(TimerFormat.Whole, "0");
        timeFormats.Add(TimerFormat.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormat.HundredthsDecimal, "0.00");
    }

    #region Timer Methods
    void IncreaseTime(float delta)
    {
        CurrentTime += delta;
        if (hasTimeLimit && CurrentTime > timeLimit) CurrentTime = timeLimit;
    }

    void DecreaseTime(float delta)
    {
        CurrentTime -= delta;
        if (CurrentTime < 0.0f) CurrentTime = 0.0f;
        if (hasTimeLimit && CurrentTime < timeLimit) CurrentTime = timeLimit;
    }

    public void SetTimer(float newTime) => CurrentTime = newTime;
    
    public void ResetTimer() => CurrentTime = timeLimit;
    #endregion

    void UpdateTimerText()
    {
        // Get the current timer format
        TimerFormat currentTimerFormat = customFormat ? GetTimerFormat() : TimerFormat.Whole;

        // Set the timer text and color
        timerText.text  = CurrentTime.ToString(timeFormats[currentTimerFormat], CultureInfo.InvariantCulture);
        timerText.color = CurrentTime <= colorSwitchValue ? Color.red : new (0.86f, 0.86f, 0.86f);
        
        // If the timer has finished, invoke the OnTimerEnded event
        if (CurrentTime <= 0.0f && !Finished) OnTimerEnded?.Invoke();
    }

    TimerFormat GetTimerFormat() => CurrentTime < hundredthsSwitchValue ? TimerFormat.HundredthsDecimal : CurrentTime < tenthSwitchValue ? TimerFormat.TenthDecimal : TimerFormat.Whole;
}