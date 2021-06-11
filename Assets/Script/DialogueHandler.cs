using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueHandler : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameManager _manager;
    [SerializeField] private Animator gameplayUI;
    [SerializeField] private Transform dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI textContent;
    [SerializeField] private Image characterSprite;

    [Header("Dialogue")]
    public DialogueData currentDialogueSet;
    [SerializeField] private int currentPage = 1;
    public event EventHandler dialogueInteractEvent;
    public event EventHandler dialogueUninteractEvent;
    
    void Start() {
        dialogueInteractEvent += enableDialogue;
        dialogueUninteractEvent += disableDialogue;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.E)) {
            CallEvent();
        }
    }

    public void CallEvent() {
        dialogueInteractEvent?.Invoke(this, EventArgs.Empty);
    }

    void enableDialogue(object sender, EventArgs e) {
        dialogueUI.gameObject.SetActive(true);
        gameplayUI.Play("GameplayUI_FadeOut");
        dialogueInteractEvent -= enableDialogue;
        dialogueInteractEvent += updateDialogue;
        currentPage = 1;
        speakerName.text = currentDialogueSet.speaker[currentDialogueSet.data[currentPage-1].speaker-1].name;
        textContent.text = currentDialogueSet.data[currentPage-1].text;
        characterSprite.sprite = currentDialogueSet.speaker[currentDialogueSet.data[currentPage-1].speaker-1].sprite;
    }

    void disableDialogue(object sender, EventArgs e) {
        gameplayUI.Play("GameplayUI_FadeIn");
        dialogueUI.gameObject.SetActive(false);
        dialogueInteractEvent += enableDialogue;
        dialogueInteractEvent -= updateDialogue;
    }

    void updateDialogue(object sender, EventArgs e) {
        if(currentPage == currentDialogueSet.data.Length) {
            dialogueUninteractEvent?.Invoke(this, EventArgs.Empty);
        }
        else {
            currentPage++;
            currentPage = Mathf.Clamp(currentPage, 1, currentDialogueSet.data.Length);
            speakerName.text = currentDialogueSet.speaker[currentDialogueSet.data[currentPage-1].speaker-1].name;
            textContent.text = currentDialogueSet.data[currentPage-1].text;
            characterSprite.sprite = currentDialogueSet.speaker[currentDialogueSet.data[currentPage-1].speaker-1].sprite;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "DialogueTrigger") {
            currentDialogueSet = col.GetComponent<DialogueTrigger>().thisData;
            CallEvent();
            Destroy(col.gameObject);
        }
    }
}
