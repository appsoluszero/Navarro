using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroText_Sequence : MonoBehaviour
{
    [SerializeField] private string[] textEachLine;
    private float[] lengthEachLine;
    private int[] currentLine;
    [SerializeField] private float timeEachLetter;

    void Start() {
        CodeGenerator();
        FindLength();
    }

    public void StartIntroText() {
        StartCoroutine(TextCoroutine());
    }

    public void StopIntroText() {
        StartCoroutine(TextDecreaseCoroutine());
    }

    void FindLength() {
        lengthEachLine = new float[textEachLine.Length];
        currentLine = new int[textEachLine.Length];
        for(int i = 0 ; i < textEachLine.Length ; ++i) {
            lengthEachLine[i] = textEachLine[i].Length;
            currentLine[i] = 0;
        }
    }

    void CodeGenerator() {
        
    }

    IEnumerator TextCoroutine() {
        bool isChanged = true;
        while(isChanged) {
            isChanged = false;
            for(int i = 0 ; i < textEachLine.Length ; ++i) {
                if(currentLine[i] < textEachLine[i].Length) {
                    transform.GetChild(i).GetComponent<TextMeshProUGUI>().text += textEachLine[i][currentLine[i]];
                    currentLine[i]++;
                    isChanged = true;
                }
            }
            yield return new WaitForSeconds(timeEachLetter);
        }
    }

    IEnumerator TextDecreaseCoroutine() {
        bool isChanged = true;
        while(isChanged) {
            isChanged = false;
            for(int i = 0 ; i < textEachLine.Length ; ++i) {
                if(currentLine[i] >= 0) {
                    transform.GetChild(i).GetComponent<TextMeshProUGUI>().text = transform.GetChild(i).GetComponent<TextMeshProUGUI>().text.Substring(0, currentLine[i]);
                    currentLine[i]--;
                    isChanged = true;
                }
            }
            yield return new WaitForSeconds(timeEachLetter);
        }
        transform.gameObject.SetActive(false);
    }
}
