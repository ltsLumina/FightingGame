﻿using UnityEngine;

[CreateAssetMenu(fileName = "JumpStateData", menuName = "State Data/JumpState Data", order = 0)]
public class JumpStateData : ScriptableObject
{
    [SerializeField] float jumpForce;
    public float JumpForce => jumpForce;
}