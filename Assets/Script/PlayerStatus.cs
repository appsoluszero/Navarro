using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private GameManager _manager;
    [Header("Player Base Status")]
    public int maxPlayerHealth = 5; 
    public float maxStamina = 20;

    [Header("Current Status")]
    public State worldState = State.Stand;
    public State playerState = State.Idle;
    public int currentHealth;
    public float currentStamina;
    public int bulletCount = 5;

    [Header("Status Parameters")]
    public float staminaRegenPerSecond = 2f;
    public event EventHandler<StatChangeEventArgs> staminaDecreaseHandler;
    public event EventHandler<StatChangeEventArgs> healthDecreaseHandler;
    public event EventHandler<StatChangeEventArgs> healthIncreaseHandler;
    public class StatChangeEventArgs : EventArgs {
        public int healthUse;
        public float staminaUse;
    }

    [SerializeField] private bool debugModeActivated;

    [HideInInspector] public bool isStaminaRegenerating, waitingForNextTick;
    private Animator _playerAnimation;

    void Start() {
        currentHealth = maxPlayerHealth;
        currentStamina = maxStamina;
        _playerAnimation = transform.GetChild(2).GetComponent<Animator>();
        GetComponent<PlayerAction>().staminaHandler += DecreaseStamina;
    }

    void Update() {
        if(debugModeActivated) {
            if(Input.GetKeyDown(KeyCode.Keypad1)) {
                DecreaseHealth(1);
            }
        }
    }

    void FixedUpdate() {
        if(isStaminaRegenerating) {
            currentStamina = Mathf.Clamp(currentStamina + (staminaRegenPerSecond / (1 / Time.fixedDeltaTime)) , 0, maxStamina);
        }
    }

    public void DecreaseHealth(int amt) {
        if(currentHealth > 0) {
            healthDecreaseHandler?.Invoke(this, new StatChangeEventArgs {
                healthUse = amt
            });
        }
        currentHealth = Mathf.Clamp(currentHealth - amt, 0, maxPlayerHealth);
        if(currentHealth == 0) 
            PlayerDieEvent();
    }

    void IncreaseHealth(int amt) {
        if(currentHealth < maxPlayerHealth) {
            healthIncreaseHandler?.Invoke(this, new StatChangeEventArgs {
                healthUse = amt
            });
        }
        currentHealth = Mathf.Clamp(currentHealth + amt, 0, maxPlayerHealth);
    }

    void DecreaseStamina(object sender, StatChangeEventArgs e) {
        isStaminaRegenerating = false;
        staminaDecreaseHandler?.Invoke(this, new StatChangeEventArgs {
            staminaUse = e.staminaUse
        });
        currentStamina = Mathf.Clamp(currentStamina - e.staminaUse, 0, maxStamina);
    }

    public void DecreaseBullet() {
        bulletCount--;
    }

    void PlayerDieEvent() {
        GetComponent<PlayerStatus>().playerState = State.Death;
        _manager.currentGameState = gameState.Ending;
        _playerAnimation.Play("Dying");
        transform.GetChild(3).gameObject.SetActive(true);
    }
}

public enum State {
    Stand,
    Crouch,
    Floating_Stand,
    Floating_Crouch,
    Idle,
    Move,
    Rolling,
    Death,
    Hurt,
    MeleeAttack,
    RangedAttack
}