using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public Transform playerPos;
    private SpriteRenderer sprite;
    [Tooltip("Number of fixed update between when that the shader sync player postion")]
    public int updateInterval = 1;
    [Tooltip("Radius that player has to be nearby for the grass to be consider active")]
    public float keepAliveRadius = 0.2f;
    private Material defaultMat;
    [Tooltip("Material used when grass is not active")]
    public Material staticMaterial;
    public LayerMask grassLayer;
    private Coroutine updaterRoutine;

    // Start is called before the first frame update
    void Start()
    {
        this.sprite = GetComponent<SpriteRenderer>();
        this.defaultMat = this.sprite.material;
        if (this.playerPos == null) {
            var player =  GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                this.playerPos = player.transform.Find("BodyPart");
            }
            else {
                Debug.LogError("playerPos is not set and cannot find object with tag `Player`");
            }
        }
        TriggerRoutine();
    }

    private void TriggerRoutine() {
        if(updaterRoutine == null) {
            this.updaterRoutine = StartCoroutine(UpdatePlayerPosRoutine());
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator UpdatePlayerPosRoutine() {
        this.sprite.material = this.defaultMat;
        var mat = this.sprite.material;
        while (true)
        {
            var pos = this.playerPos.position;
            mat.SetVector("_PlayerPos", pos);

            if(Vector2.Distance(transform.position, pos) > this.keepAliveRadius) {
                break;
            }

            for (int i = 0; i < this.updateInterval; i++)
            {
                yield return new WaitForFixedUpdate();   
            }
        }
        this.sprite.material = this.staticMaterial;
        this.updaterRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            this.TriggerRoutine();
            var nearbyGrass = Physics2D.OverlapCircleAll(transform.position, keepAliveRadius, grassLayer);
            foreach (var grass in nearbyGrass)
            {
                var grassComponent = grass.GetComponent<Grass>();
                if(grassComponent == null) {
                    continue;
                }
                grassComponent.TriggerRoutine();
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, keepAliveRadius);
    }
}
