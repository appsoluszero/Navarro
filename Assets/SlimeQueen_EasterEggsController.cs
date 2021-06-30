using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SlimeQueen_EasterEggsController : MonoBehaviour
{
    public PlayableAsset slimeQueen;
    public PlayableDirector _director;
    float countDown = 0;

    void Update() {
        if(countDown < 60f) {
            countDown += Time.deltaTime;
            countDown = Mathf.Clamp(countDown, 0f, 60f);
        }

        if(countDown == 60f) {
            _director.playableAsset = slimeQueen;
            _director.Play();
            countDown = 0f;
        }
    }
}
