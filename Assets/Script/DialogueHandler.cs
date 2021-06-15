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
    [SerializeField] private Animator dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI textContent;
    [SerializeField] private Image characterSprite;

    [Header("Dialogue")]
    public DialogueData currentDialogueSet;
    [SerializeField] private int currentPage = 1;
    public event EventHandler dialogueInteractEvent;
    public event EventHandler dialogueUninteractEvent;
    private PlayerCameraController _camController;
    
    void Start() {
        _camController = transform.GetChild(0).GetComponent<PlayerCameraController>();
        dialogueInteractEvent += enableDialogue;
        dialogueUninteractEvent += disableDialogue;
    }

    void Update() {
        if(_manager.currentGameState == gameState.Dialogue && Input.GetKeyDown(KeyCode.E)) {
            CallEvent();
        }
    }

    public void CallEvent() {
        dialogueInteractEvent?.Invoke(this, EventArgs.Empty);
    }

    void enableDialogue(object sender, EventArgs e) {
        dialogueUI.Play("DialogueUI_FadeIn");
        gameplayUI.Play("GameplayUI_FadeOut");
        dialogueInteractEvent -= enableDialogue;
        dialogueInteractEvent -= _camController.DialogueStateCamera;
        dialogueInteractEvent += updateDialogue;
        currentPage = 1;
        speakerName.text = currentDialogueSet.speaker[currentDialogueSet.data[currentPage-1].speaker-1].name;
        textContent.text = currentDialogueSet.data[currentPage-1].text;
        characterSprite.sprite = currentDialogueSet.speaker[currentDialogueSet.data[currentPage-1].speaker-1].sprite;
    }

    void disableDialogue(object sender, EventArgs e) {
        dialogueUI.Play("DialogueUI_FadeOut");
        gameplayUI.Play("GameplayUI_FadeIn");
        dialogueInteractEvent -= updateDialogue;
        dialogueInteractEvent += enableDialogue;
        dialogueInteractEvent += _camController.DialogueStateCamera;
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
