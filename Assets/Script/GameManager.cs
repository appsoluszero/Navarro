using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Reference
    [SerializeField] private DialogueHandler _dialogue;
    [SerializeField] private int targetFrameRate = 60;
    public gameState currentGameState;
    void Start()
    {
        currentGameState = gameState.Intro;
        Application.targetFrameRate = targetFrameRate;

        _dialogue.dialogueInteractEvent += ChangeStateToDialogue;
        _dialogue.dialogueUninteractEvent += ChangeStateToGameplay;
    }

    void ChangeStateToDialogue(object sender, EventArgs e) {
        currentGameState = gameState.Dialogue;
    }

    void ChangeStateToGameplay(object sender, EventArgs e) {
        currentGameState = gameState.Gameplay;
    }
}

public enum gameState {
    Menu,
    Gameplay,
    Dialogue,
    Ending,
    Intro
}
