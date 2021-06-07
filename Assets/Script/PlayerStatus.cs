using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [Header("User Interface")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Image staminaImage;

    [Header("Player Base Status")]
    [SerializeField] private float maxPlayerHealth; 
    [SerializeField] private float maxStamina;

    [Header("Current Status")]
    public State worldState = State.Stand;
    public State playerState = State.Idle;
    public float currentHealth;
    public float currentStamina;

    public class StatDecreaseEventArgs : EventArgs {
        public float healthUse;
        public float staminaUse;
        
    }

    void Start() {
        currentHealth = maxPlayerHealth;
        currentStamina = maxStamina;
        GetComponent<PlayerAction>().staminaBarHandler += DecreaseStamina;
    }
    
    void DecreaseStamina(object sender, StatDecreaseEventArgs e) {
        currentStamina -= e.staminaUse;
        staminaImage.fillAmount = currentStamina / maxStamina;
    }
}

public enum State {
    Stand,
    Crouch,
    Floating_Stand,
    Floating_Crouch,
    Idle,
    Move,
    Attack,
    Rolling,
    Sliding
}