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
    public State worldState = State.Standing;
    public State playerState = State.Idle;
    public float currentHealth;
    public float currentStamina;

    float healthImageSpeed;
    float staminaImageSpeed;

    void Start() {
        currentHealth = maxPlayerHealth;
        currentStamina = maxStamina;
    }

    void Update() {
        healthImage.fillAmount = Mathf.SmoothDamp(healthImage.fillAmount, currentHealth / maxPlayerHealth, ref healthImageSpeed, 0.2f, 2f);
        healthImage.fillAmount = Mathf.Clamp(healthImage.fillAmount, 0, 1);
    }

    public IEnumerator decreaseStamina() {
        currentStamina -= 20f;
        while(staminaImage.fillAmount > currentStamina / maxStamina) {
            staminaImage.fillAmount = Mathf.SmoothDamp(staminaImage.fillAmount, currentStamina/maxStamina, ref staminaImageSpeed, 0.2f, 3f);
            staminaImage.fillAmount = Mathf.Clamp(staminaImage.fillAmount, 0, 1);
            yield return new WaitForEndOfFrame();
        }
    }
}

public enum State {
    Standing,
    Crouching,
    Floating_Standing,
    Floating_Crouching,
    Idle,
    Move,
    Attack,
    Rolling,
    Sliding
}