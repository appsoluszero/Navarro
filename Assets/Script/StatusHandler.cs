using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusHandler : MonoBehaviour
{
    [Header("User Interface/Health")]
    [SerializeField] private GameObject healthForeground_Prefab;
    [SerializeField] private RectTransform health_Foreground;
    [SerializeField] private GameObject healthBackground_Prefab;
    [SerializeField] private RectTransform health_Background;
    [Header("User Interface/Stamina")]
    [SerializeField] private RectTransform stamina_Foreground;
    [SerializeField] private RectTransform stamina_Background;
    [Header("Animation Parameter")]
    [SerializeField] private float timeToChange = 0.3f;
    [SerializeField] private AnimationCurve curve;
    
    private PlayerStatus _status;
    private PlayerAction _action;
    List<int> queueHealthChange;
    bool isHealthDecreasing;
    bool isHealthIncreasing;

    void Start() {
        _status = GetComponent<PlayerStatus>();
        _action = GetComponent<PlayerAction>();
        SetUpHealthBar();
        _status.healthDecreaseHandler += DecreaseHealthUI;
        _status.healthIncreaseHandler += IncreaseHealthUI;
        _status.staminaDecreaseHandler += DecreaseStaminaUI;
        queueHealthChange = new List<int>();
    }

    void Update() {
        if(_status.isStaminaRegenerating)
            stamina_Foreground.GetComponent<Image>().fillAmount = _status.currentStamina / _status.maxStamina;
    }

    void DecreaseStaminaUI(object sender, PlayerStatus.StatChangeEventArgs e) {
        StartCoroutine(startStaminaChange(e));
    }

    IEnumerator startStaminaChange(PlayerStatus.StatChangeEventArgs e) {
        float time = 0f;
        float now = _status.currentStamina / _status.maxStamina;
        float target = (_status.currentStamina - e.staminaUse) / _status.maxStamina;
        while(time <= _action.timeToFinishRoll) {
            time += Time.deltaTime;
            float progress = time / _action.timeToFinishRoll;
            stamina_Foreground.GetComponent<Image>().fillAmount = Mathf.Lerp(now, target, curve.Evaluate(progress));
            yield return new WaitForEndOfFrame();
        }
        _status.isStaminaRegenerating = true;
    }

    void DecreaseHealthUI(object sender, PlayerStatus.StatChangeEventArgs e) {
        for(int i = _status.currentHealth ; i > Mathf.Clamp(_status.currentHealth - e.healthUse, 0, _status.maxPlayerHealth) ; --i) {
            if(isHealthIncreasing) {
                isHealthIncreasing = false;
                queueHealthChange.Clear();
                StopAllCoroutines();
            }
            queueHealthChange.Add(i-1);
            if(queueHealthChange.Count == 1) {
                isHealthDecreasing = true;
                StartCoroutine(startHealthChange(1));
            }
        }
    }

    void IncreaseHealthUI(object sender, PlayerStatus.StatChangeEventArgs e) {
        for(int i = _status.currentHealth ; i < Mathf.Clamp(_status.currentHealth + e.healthUse, 0, _status.maxPlayerHealth) ; ++i) {
            if(isHealthDecreasing) {
                isHealthDecreasing = false;
                queueHealthChange.Clear();
                StopAllCoroutines();
            }
            queueHealthChange.Add(i);
            if(queueHealthChange.Count == 1) {
                isHealthIncreasing = true;
                StartCoroutine(startHealthChange(2));
            }
        }
    }

    void SetUpHealthBar() {
        GameObject obj_bg = new GameObject();
        GameObject obj = new GameObject();
        for(int i = 1 ; i < _status.maxPlayerHealth - 1 ; ++i) {
            obj_bg = Instantiate(healthBackground_Prefab);
            obj = Instantiate(healthForeground_Prefab);
            obj_bg.transform.SetParent(health_Background);
            obj.transform.SetParent(health_Foreground);
            obj_bg.transform.SetSiblingIndex(i);
            obj.transform.SetSiblingIndex(i);
            obj_bg.GetComponent<RectTransform>().position = health_Background.GetChild(0).GetComponent<RectTransform>().position + new Vector3(0, (health_Background.GetChild(0).GetComponent<RectTransform>().sizeDelta.x / 2f) + (obj_bg.GetComponent<RectTransform>().sizeDelta.x / 2f * (2*i - 1)), 0);
            obj.GetComponent<RectTransform>().position = health_Foreground.GetChild(0).GetComponent<RectTransform>().position + new Vector3(0, (health_Foreground.GetChild(0).GetComponent<RectTransform>().sizeDelta.x / 2f) + (obj.GetComponent<RectTransform>().sizeDelta.x / 2f * (2*i - 1)), 0);
        }
        health_Background.GetChild(_status.maxPlayerHealth - 1).GetComponent<RectTransform>().position = obj_bg.transform.GetComponent<RectTransform>().position + new Vector3(0, (obj_bg.GetComponent<RectTransform>().sizeDelta.x / 2f) + (health_Background.GetChild(_status.maxPlayerHealth - 1).GetComponent<RectTransform>().sizeDelta.x / 2f), 0);
        health_Foreground.GetChild(_status.maxPlayerHealth - 1).GetComponent<RectTransform>().position = obj.transform.GetComponent<RectTransform>().position + new Vector3(0, (obj_bg.GetComponent<RectTransform>().sizeDelta.x / 2f) + (health_Foreground.GetChild(_status.maxPlayerHealth - 1).GetComponent<RectTransform>().sizeDelta.x / 2f), 0);
    }

    IEnumerator startHealthChange(int type) {
        int now = -1;
        float target = -1f;
        now = queueHealthChange[0];
        if(type == 1) target = 0f;
        else if(type == 2) target = 1f;
        float time = 0f;
        float currFillAmt = health_Foreground.GetChild(now).GetComponent<Image>().fillAmount;
        while(time <= timeToChange) {
            time += Time.deltaTime;
            float progress = time / timeToChange;
            health_Foreground.GetChild(now).GetComponent<Image>().fillAmount = Mathf.Lerp(currFillAmt, target, progress);
            yield return new WaitForEndOfFrame();
        }
        queueHealthChange.RemoveAt(0);
        if(type == 1) {
            if(queueHealthChange.Count != 0) 
                StartCoroutine(startHealthChange(1));
            else isHealthDecreasing = false;
        } 
        else if(type == 2) {
            if(queueHealthChange.Count != 0) 
                StartCoroutine(startHealthChange(2));
            else isHealthIncreasing = false;
        } 
    }

    
}
