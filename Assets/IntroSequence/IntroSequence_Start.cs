using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class IntroSequence_Start : MonoBehaviour
{
    [SerializeField] private Animator gameplayUI;
    [SerializeField] private GameManager _manager;
    [SerializeField] private GameObject spawnRoom;
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private PlayableAsset lastSequence;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AudioSource _audioSource;

    public void ActuallyStarting() {
        _manager.currentGameState = gameState.Gameplay;
        gameplayUI.Play("GameplayUI_FadeIn");
        playerTransform.position = transform.GetChild(3).position;
        playerTransform.GetChild(0).gameObject.SetActive(true);
        spawnRoom.SetActive(false);
        transform.gameObject.SetActive(false);
        _audioSource.Play();
    }

    public void ChangeToLastSequence() {
        _director.playableAsset = lastSequence;
        _director.Play();
    }
}
