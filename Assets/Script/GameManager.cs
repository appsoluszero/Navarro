using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;
    public gameState currentGameState;
    void Start()
    {
        currentGameState = gameState.Gameplay;
        Application.targetFrameRate = targetFrameRate;
    }
}

public enum gameState {
    Menu,
    Gameplay,
    Dialogue
}
