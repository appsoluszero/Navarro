using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroText_Sequence : MonoBehaviour
{
    private static char[] alphabet = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
    private static char[] num = {'1','2','3','4','5','6','7','8','9','0'};
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
        string code_alphabet = "" + alphabet[Random.Range(1, 28)-1] + alphabet[Random.Range(1, 28)-1];
        string code_num = "" + num[Random.Range(1, 11)-1] + num[Random.Range(1, 11)-1] + num[Random.Range(1, 11)-1];
        textEachLine[3] = "Subject: " + code_alphabet + "-" + code_num + " \"Lunette\"";
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
