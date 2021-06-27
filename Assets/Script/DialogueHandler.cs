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
    [SerializeField] private int currentPage = 1;

    public class dialogueDataEventArgs : EventArgs {
        public DialogueData dialogue;
    }
    public dialogueDataEventArgs dialogueEventArgs;

    public event EventHandler<dialogueDataEventArgs> dialogueInteractEvent;
    public event EventHandler dialogueUninteractEvent;
    private PlayerCameraController _camController;
    
    void Start() {
        _camController = transform.GetChild(0).GetComponent<PlayerCameraController>();
        dialogueInteractEvent += enableDialogue;
        dialogueUninteractEvent += disableDialogue;
        dialogueEventArgs = new dialogueDataEventArgs();
    }

    void Update() {
        if(_manager.currentGameState == gameState.Dialogue && Input.GetKeyDown(KeyCode.E)) {
            CallEvent(null);
        }
    }

    public void CallEvent(DialogueData thisData) {
        if(thisData != null) {
            dialogueEventArgs.dialogue = thisData;
        }
        dialogueInteractEvent?.Invoke(this, dialogueEventArgs);
    }

    void enableDialogue(object sender, dialogueDataEventArgs e) {
        dialogueUI.Play("DialogueUI_FadeIn");
        gameplayUI.Play("GameplayUI_FadeOut");
        dialogueInteractEvent -= enableDialogue;
        dialogueInteractEvent -= _camController.DialogueStateCamera;
        dialogueInteractEvent += updateDialogue;
        currentPage = 1;
        speakerName.text = e.dialogue.speaker[e.dialogue.data[currentPage-1].speaker-1].name;
        textContent.text = e.dialogue.data[currentPage-1].text;
        characterSprite.sprite = e.dialogue.speaker[e.dialogue.data[currentPage-1].speaker-1].sprite;
    }

    void disableDialogue(object sender, EventArgs e) {
        dialogueUI.Play("DialogueUI_FadeOut");
        gameplayUI.Play("GameplayUI_FadeIn");
        dialogueInteractEvent -= updateDialogue;
        dialogueInteractEvent += enableDialogue;
        dialogueInteractEvent += _camController.DialogueStateCamera;
    }

    void updateDialogue(object sender, dialogueDataEventArgs e) {
        if(currentPage == e.dialogue.data.Length) {
            dialogueUninteractEvent?.Invoke(this, EventArgs.Empty);
        }
        else {
            currentPage++;
            currentPage = Mathf.Clamp(currentPage, 1, e.dialogue.data.Length);
            speakerName.text = e.dialogue.speaker[e.dialogue.data[currentPage-1].speaker-1].name;
            textContent.text = e.dialogue.data[currentPage-1].text;
            characterSprite.sprite = e.dialogue.speaker[e.dialogue.data[currentPage-1].speaker-1].sprite;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "DialogueTrigger") {
            CallEvent(col.transform.GetComponent<DialogueTrigger>().thisData);
            Destroy(col.gameObject);
        }
    }
}
