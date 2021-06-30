using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyType;
    [SerializeField] private Vector3[] spawnPoint;
    [SerializeField] private Transform playerRef;
    private float cooldownTimer;
    bool isCountingDown = false;

    void Awake() {
        spawnPoint = new Vector3[transform.childCount];
        for(int i = 0 ; i < spawnPoint.Length ; ++i) {
            spawnPoint[i] = transform.GetChild(i).position;
        }
    }

    void Update() {
        if(!isCountingDown) {
            cooldownTimer = Random.Range(4f, 12f);
            isCountingDown = true;
            StartCoroutine(spawnMonster());
        }
    }

    IEnumerator spawnMonster() {
        yield return new WaitForSeconds(cooldownTimer);
        SpawnStuff();
        isCountingDown = false;
    }

    public void SpawnStuff() {
        GameObject p = Instantiate(enemyType[Random.Range(0, enemyType.Length)], spawnPoint[Random.Range(0, spawnPoint.Length)], new Quaternion());
        p.GetComponent<ArtificialIntelligence>().SetTarget(playerRef);
    }
}
