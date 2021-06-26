using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootstepSound : MonoBehaviour
{
    [SerializeField] private LayerMask levelAndPlatformMask;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip[] footStepConcrete;
    [SerializeField] private AudioClip footStepDirt;

    public void PlayFootStep() {
        RaycastHit2D hit = Physics2D.Raycast(transform.parent.position, Vector2.down, Mathf.Infinity, levelAndPlatformMask);
        if(hit) {
            if(hit.transform.gameObject.CompareTag("Level_Concrete")) {
                _audio.PlayOneShot(footStepConcrete[Random.Range(0, footStepConcrete.Length)], 1f);
            }
        }
    }
}
