using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class LoadManager : MonoBehaviour
{
    public PlayableAsset duringLoadAsset;
    public PlayableDirector loadSequenceController;
    private AsyncOperation loadOperation;
    public void StartLoadingScene() {
        loadSequenceController.Play();
    }

    public void startLoading() {
        loadSequenceController.playableAsset = duringLoadAsset;
        loadSequenceController.extrapolationMode = DirectorWrapMode.Loop;
        loadSequenceController.Play();
        StartCoroutine(loadSceneCoroutine());
    }

    IEnumerator loadSceneCoroutine() {
        loadOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;
        while(!loadOperation.isDone) {
            print(loadOperation.progress);
            yield return null;
        }
        loadOperation = SceneManager.UnloadSceneAsync(0);
        while(!loadOperation.isDone) {
            yield return null;
        }
    }

    public void setReadyNextScene() {
        if(loadOperation.progress == 0.9f) {
            loadOperation.allowSceneActivation = true;
        }
    }
}
