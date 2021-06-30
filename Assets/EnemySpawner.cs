using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyType;
    [SerializeField] private Vector3[] spawnPoint;

    void Start() {
        spawnPoint = new Vector3[transform.childCount];
        for(int i = 0 ; i < spawnPoint.Length ; ++i) {
            spawnPoint[i] = transform.GetChild(i).position;
        }
    }
}
