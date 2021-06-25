using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public PlayerMovement playerPos;
    private Material material;
    public float updateFrequency = 0.5f;
    public float infulencePower = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        this.material = GetComponent<SpriteRenderer>().material;
        if (this.playerPos == null) {
            var player =  GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                this.playerPos = player.GetComponent<PlayerMovement>();
            }
            else {
                Debug.LogError("playerPos is not set and cannot find object with tag `Player`");
            }
        }
        // StartCoroutine(UpdatePlayerPosRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        this.material.SetVector("_PlayerPos", playerPos.transform.position);
        this.material.SetVector("_PlayerVelocity", playerPos.velocity);
    }

    IEnumerator UpdatePlayerPosRoutine() {
        while (true)
        {
            this.material.SetVector("_PlayerPos", playerPos.transform.position);
            this.material.SetVector("_PlayerVelocity", playerPos.velocity);
            yield return new WaitForFixedUpdate();
        }
    }
}
