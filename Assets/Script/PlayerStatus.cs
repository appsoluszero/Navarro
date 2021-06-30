using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public class StatChangeEventArgs : EventArgs
    {
        public int healthUse;
        public float staminaUse;
    }

    [SerializeField] private bool debugModeActivated;

    [HideInInspector] public bool isStaminaRegenerating, waitingForNextTick;
    private Animator _playerAnimation;
    private Controller2D _controller;

    void Start()
    {
        currentHealth = maxPlayerHealth;
        currentStamina = maxStamina;
        _playerAnimation = transform.GetChild(2).GetComponent<Animator>();
        _controller = GetComponent<Controller2D>();
        GetComponent<PlayerAction>().staminaHandler += DecreaseStamina;
    }
    void Update()
    {
        if (debugModeActivated)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                DecreaseHealth(1);
            }
        }

    }

    void FixedUpdate()
    {
        if (isStaminaRegenerating)
        {
            currentStamina = Mathf.Clamp(currentStamina + (staminaRegenPerSecond / (1 / Time.fixedDeltaTime)), 0, maxStamina);
        }
    }


    public void DecreaseHealth(int amt)
    {
        if (currentHealth > 0)
        {
            healthDecreaseHandler?.Invoke(this, new StatChangeEventArgs
            {
                healthUse = amt
            });
        }
        currentHealth = Mathf.Clamp(currentHealth - amt, 0, maxPlayerHealth);
        if (currentHealth == 0)
            PlayerDieEvent();
    }

    public void IncreaseHealth(int amt)
    {
        if (currentHealth < maxPlayerHealth)
        {
            healthIncreaseHandler?.Invoke(this, new StatChangeEventArgs
            {
                healthUse = amt
            });
        }
        currentHealth = Mathf.Clamp(currentHealth + amt, 0, maxPlayerHealth);
    }


    void DecreaseStamina(object sender, StatChangeEventArgs e)
    {

        isStaminaRegenerating = false;
        staminaDecreaseHandler?.Invoke(this, new StatChangeEventArgs
        {
            staminaUse = e.staminaUse
        });
        currentStamina = Mathf.Clamp(currentStamina - e.staminaUse, 0, maxStamina);
    }

    public void DecreaseBullet()
    {
        bulletCount--;
    }

    public void GainBullet() {
        bulletCount++;
    }

    void PlayerDieEvent()
    {
        GetComponent<PlayerStatus>().playerState = State.Death;
        _manager.currentGameState = gameState.Ending;
        _playerAnimation.Play("Dying");
        transform.GetChild(3).gameObject.SetActive(true);
        transform.GetChild(3).localScale = new Vector3(_controller.collision.faceDir, 1f, 1f);
        transform.GetChild(4).gameObject.SetActive(true);
        StartCoroutine(countDownBackMainMenu());
    }

    IEnumerator countDownBackMainMenu() {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}

public enum State
{
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