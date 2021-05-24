using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Player Base Status")]
    [SerializeField] private float maxPlayerHealth; 
    [SerializeField] private float maxStamina;

    [Header("Current Status")]
    public State playerState;
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentStamina;
}

public enum State {
    Idle,
    Move,
    Attack
}