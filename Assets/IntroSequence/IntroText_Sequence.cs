using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroText_Sequence : MonoBehaviour
{
    [SerializeField] private string[] textEachLine;
    [SerializeField] private float timeEachLetter;

    public void StartIntroText() {
        StartCoroutine(TextCoroutine());
    }

    IEnumerator TextCoroutine() {
        int currentLetter = 0;
        bool isChanged = true;
        while(isChanged) {
            isChanged = false;
            for(int i = 0 ; i < textEachLine.Length ; ++i) {
                if(currentLetter < textEachLine[i].Length) {
                    transform.GetChild(i).GetComponent<TextMeshProUGUI>().text += textEachLine[i][currentLetter];
                    isChanged = true;
                }
            }
            yield return new WaitForSeconds(timeEachLetter);
            currentLetter++;
        }
    }
}
