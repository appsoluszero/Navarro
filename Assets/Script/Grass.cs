using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public Transform playerPos;
    private Material material;
    public float updateFrequency = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        this.material = GetComponent<SpriteRenderer>().material;
        if (this.playerPos == null) {
            var player =  GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                this.playerPos = player.transform.Find("BodyPart");
            }
            else {
                Debug.LogError("playerPos is not set and cannot find object with tag `Player`");
            }
        }
        StartCoroutine(UpdatePlayerPosRoutine());
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator UpdatePlayerPosRoutine() {
        while (true)
        {
            this.material.SetVector("_PlayerPos", this.playerPos.position);
            
            yield return new WaitForFixedUpdate();
        }
    }
}
